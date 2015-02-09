// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using AutoMapper.Mappers;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Maps planning poker domain entities to planning poker service data entities.
    /// </summary>
    internal static class ServiceEntityMapper
    {
        #region Fields

        private static readonly Lazy<IConfigurationProvider> Configuration = new Lazy<IConfigurationProvider>(CreateMapperConfiguration);
        private static readonly Lazy<IMappingEngine> MappingEngine = new Lazy<IMappingEngine>(() => new MappingEngine(Configuration.Value));

        #endregion

        #region Public methods

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

        #endregion

        #region Private methods

        private static IConfigurationProvider CreateMapperConfiguration()
        {
            var result = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);

            result.CreateMap<D.ScrumTeam, ScrumTeam>();
            result.CreateMap<D.Observer, TeamMember>()
                .ForMember(m => m.Type, mc => mc.ResolveUsing((D.Observer o) => o.GetType().Name));
            result.CreateMap<D.Message, Message>()
                .Include<D.MemberMessage, MemberMessage>()
                .Include<D.EstimationResultMessage, EstimationResultMessage>()
                .ForMember(m => m.Type, mc => mc.MapFrom(m => m.MessageType));
            result.CreateMap<D.MemberMessage, MemberMessage>();
            result.CreateMap<D.EstimationResultMessage, EstimationResultMessage>();
            result.CreateMap<KeyValuePair<D.Member, D.Estimation>, EstimationResultItem>()
                .ForMember(i => i.Member, mc => mc.MapFrom(p => p.Key))
                .ForMember(i => i.Estimation, mc => mc.MapFrom(p => p.Value));
            result.CreateMap<D.Estimation, Estimation>()
                .ForMember(e => e.Value, mc => mc.ResolveUsing(e => e.Value.HasValue && double.IsPositiveInfinity(e.Value.Value) ? Estimation.PositiveInfinity : e.Value));

            result.AssertConfigurationIsValid();

            return result;
        }

        #endregion
    }
}
