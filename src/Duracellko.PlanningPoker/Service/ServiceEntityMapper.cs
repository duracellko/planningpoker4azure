using System;
using System.Collections.Generic;
using AutoMapper;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Maps planning poker domain entities to planning poker service data entities.
    /// </summary>
    internal static class ServiceEntityMapper
    {
        private static readonly Lazy<IConfigurationProvider> Configuration = new Lazy<IConfigurationProvider>(CreateMapperConfiguration);
        private static readonly Lazy<IMapper> MappingEngine = new Lazy<IMapper>(() => new Mapper(Configuration.Value));

        /// <summary>
        /// Maps the specified source entity to destination entity.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <typeparam name="TDestination">The type of the destination entity.</typeparam>
        /// <param name="source">The source entity to map.</param>
        /// <returns>The mapped destination entity.</returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return MappingEngine.Value.Map<TSource, TDestination>(source);
        }

        /// <summary>
        /// Filters or transforms message before sending to client.
        /// MemberDisconnected message of ScrumMaster is transformed to Empty message,
        /// because that is internal message and ScrumMaster is not actually disconnected.
        /// </summary>
        /// <param name="message">The message to transform.</param>
        /// <returns>The transformed message.</returns>
        public static D.Message FilterMessage(D.Message message)
        {
            if (message.MessageType == D.MessageType.MemberDisconnected)
            {
                var memberMessage = (D.MemberMessage)message;
                if (memberMessage.Member is D.ScrumMaster)
                {
                    return new D.Message(D.MessageType.Empty, message.Id);
                }
            }

            return message;
        }

        private static IConfigurationProvider CreateMapperConfiguration()
        {
            var result = new MapperConfiguration(config =>
            {
                config.AllowNullCollections = true;
                config.CreateMap<D.ScrumTeam, ScrumTeam>();
                config.CreateMap<D.Observer, TeamMember>()
                    .ForMember(m => m.Type, mc => mc.MapFrom((s, d, m) => s.GetType().Name));
                config.CreateMap<D.Message, Message>()
                    .Include<D.MemberMessage, MemberMessage>()
                    .Include<D.EstimationResultMessage, EstimationResultMessage>()
                    .ForMember(m => m.Type, mc => mc.MapFrom(m => m.MessageType));
                config.CreateMap<D.MemberMessage, MemberMessage>();
                config.CreateMap<D.EstimationResultMessage, EstimationResultMessage>();
                config.CreateMap<KeyValuePair<D.Member, D.Estimation>, EstimationResultItem>()
                    .ForMember(i => i.Member, mc => mc.MapFrom(p => p.Key))
                    .ForMember(i => i.Estimation, mc => mc.MapFrom(p => p.Value));
                config.CreateMap<D.EstimationParticipantStatus, EstimationParticipantStatus>();
                config.CreateMap<D.Estimation, Estimation>()
                    .ForMember(e => e.Value, mc => mc.MapFrom((s, d, m) => MapEstimationValue(s.Value)));
            });

            result.AssertConfigurationIsValid();
            return result;
        }

        private static double? MapEstimationValue(double? value) =>
            value.HasValue && double.IsPositiveInfinity(value.Value) ? Estimation.PositiveInfinity : value;
    }
}
