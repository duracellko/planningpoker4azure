/// <reference path="./typings/jquery/jquery.d.ts"/>
/// <reference path="./typings/knockout/knockout.d.ts"/>
/// <reference path="./typings/linq/linq.d.ts"/>
/// <reference path="./typings/bootstrap/bootstrap.d.ts"/>

module Duracellko.PlanningPoker {

    export class Exception {
        private _message: string;

        constructor(message: string) {
            this._message = message;
        }

        public get message(): string {
            return this._message;
        }
    }

    export enum TeamState {
        Initial = 0,
        EstimationInProgress = 1,
        EstimationFinished = 2,
        EstimationCanceled = 3
    };

    export class ScrumTeam {
        public name: string = null;
        public scrumMaster: TeamMember = null;
        public members: TeamMember[] = null;
        public observers: TeamMember[] = null;
        public state: TeamState = null;
        public avilableEstimations: Estimation[] = null;
        public estimationResult: EstimationResultItem[] = null;
        public estimationParticipants: EstimationParticipantStatus[] = null;
    }

    export class TeamMember {
        public type: string = null;
        public name: string = null;
    }

    export class Estimation {
        public static positiveInfinity: number = -1111100.0;
        public value: number = null;
    }

    export class EstimationResultItem {
        public member: TeamMember = null;
        public estimation: Estimation = null;
    }

    export class EstimationParticipantStatus {
        public memberName: string = null;
        public estimated: boolean = null;
    }

    export enum MessageType {
        Empty,
        MemberJoined,
        MemberDisconnected,
        EstimationStarted,
        EstimationEnded,
        EstimationCanceled,
        MemberEstimated
    }

    export class Message {
        public id: number = null;
        public type: MessageType = null;
    }

    export class MemberMessage extends Message {
        public member: TeamMember = null;
    }

    export class EstimationResultMessage extends Message {
        public estimationResult: EstimationResultItem[] = null;
    }

    export class ReconnectTeamResult {
        public scrumTeam: ScrumTeam = null;
        public lastMessageId: number = 0;
        public selectedEstimation: Estimation = null;
    }

    export class MessageBoxService {
        public show(message: string, title: string = null, primaryButtonText: string = null, primaryButtonHandler: () => boolean = null): void {
            var globalMessagePanel = $("#globalMessagePanel");
            $("#globalMessageTitle", globalMessagePanel).text(title != null ? title : "");
            $("#globalMessageContainer", globalMessagePanel).text(message != null ? message : "");

            var primaryButton = $("#globalMessagePrimaryButton", globalMessagePanel);
            primaryButton.unbind("click").hide().text("");
            if (primaryButtonText != null && primaryButtonText != "") {
                primaryButton.text(primaryButtonText).show();
            }
            if (primaryButtonHandler != null) {
                primaryButton.bind("click", e => {
                    if (primaryButtonHandler()) {
                        globalMessagePanel.modal("hide");
                    }
                });
            }

            globalMessagePanel.modal(<ModalOptionsBackdropString>{ backdrop: "static" });
        }
    }

    export class PlanningPokerController {
        private container: JQuery;
        private serviceUrl: string;
        private templateUrl: string;
        private messageBoxService = new MessageBoxService();
        private service: PlanningPokerService = null;
        private userManager: UserManager = null;
        private createTeamController: CreateTeamController = null;
        private joinTeamController: JoinTeamController = null;
        private planningPokerDeskController: PlanningPokerDeskController = null;

        constructor(container: any, serviceUrl: string, templateUrl: string) {
            if (container == null) {
                throw new Exception("container");
            }
            if (serviceUrl == null || serviceUrl == "") {
                throw new Exception("serviceUrl");
            }
            if (templateUrl == null || templateUrl == "") {
                throw new Exception("templateUrl");
            }

            this.container = $(container);
            this.serviceUrl = serviceUrl;
            this.templateUrl = templateUrl;
        }

        public initialize(): void {
            this.service = this.createPlanningPokerService();
            this.userManager = new UserManager();

            var settings = <JQueryAjaxSettings>{
                dataType: "html",
                beforeSend: (jqXHR: JQueryXHR, settings: JQueryAjaxSettings) => this.JQueryAjaxOnBeforeSend(jqXHR, settings),
                complete: (jqXHR: JQueryXHR, textStatus: string) => this.JQueryAjaxOnComplete(jqXHR, textStatus),
            };

            var promise: JQueryPromise<any> = $.ajax(this.templateUrl, settings);
            promise.then((data: any): string => data).done(data => this.initializeTemplate(data));
        }

        private initializeTemplate(template: string): void {
            this.container.html(template);

            this.createTeamController = new CreateTeamController(this.service);
            this.createTeamController.onTeamCreated = (scrumTeam, userName) => this.onTeamCreated(scrumTeam, userName);
            this.joinTeamController = new JoinTeamController(this.service, this.messageBoxService, this.userManager);
            this.joinTeamController.onTeamJoined = (scrumTeam, userName) => this.onTeamJoined(scrumTeam, userName);
            this.joinTeamController.onTeamReconnected = (teamResult, userName) => this.onTeamReconnected(teamResult, userName);
            this.planningPokerDeskController = new PlanningPokerDeskController(this.service, this.userManager);
            this.planningPokerDeskController.onTeamDisconnected = (teamName, userName) => this.onTeamDisconnected(teamName, userName);

            this.initializeHomeScreen();
        }

        private createPlanningPokerService(): PlanningPokerService {
            var result = new PlanningPokerService(this.serviceUrl);
            result.onRequestBegin = jqXHR => this.serviceOnRequestBegin(jqXHR);
            result.onRequestEnd = jqXHR => this.serviceOnRequestEnd(jqXHR);
            result.onRequestError = jqXHR => this.serviceOnRequestError(jqXHR);
            return result;
        }

        private initializeHomeScreen(): void {
            this.planningPokerDeskController.hide();
            this.planningPokerDeskController.dispose();
            $("#navbarPlanningPoker", this.container).collapse("hide");

            this.createTeamController.initialize($("#createTeamPanel", this.container));
            this.joinTeamController.initialize($("#joinTeamPanel", this.container));
            this.createTeamController.show();
            this.joinTeamController.show();
        }

        private initializePlanningPokerDesk(scrumTeam: ScrumTeam, userName: string, lastMessageId: number = null, selectedEstimation: Estimation = null): void {
            this.createTeamController.hide();
            this.joinTeamController.hide();
            this.createTeamController.dispose();
            this.joinTeamController.dispose();
            $("#navbarPlanningPoker", this.container).collapse("hide");

            this.planningPokerDeskController.initialize(scrumTeam, userName, this.container, lastMessageId, selectedEstimation);
            this.planningPokerDeskController.show();
        }

        private onTeamCreated(scrumTeam: ScrumTeam, userName: string): void {
            this.initializePlanningPokerDesk(scrumTeam, userName);
        }

        private onTeamJoined(scrumTeam: ScrumTeam, userName: string): void {
            this.initializePlanningPokerDesk(scrumTeam, userName);
        }

        private onTeamReconnected(teamResult: ReconnectTeamResult, userName: string): void {
            this.initializePlanningPokerDesk(teamResult.scrumTeam, userName, teamResult.lastMessageId, teamResult.selectedEstimation);
        }

        private onTeamDisconnected(teamName: string, userName: string): void {
            this.initializeHomeScreen();
        }

        private serviceOnRequestBegin(jqXHR: JQueryXHR): void {
            $("#busyIndicatorPanel").modal(<ModalOptionsBackdropString>{ backdrop: "static", keyboard: false });
        }

        private serviceOnRequestEnd(jqXHR: JQueryXHR): void {
            $("#busyIndicatorPanel").modal("hide");
        }

        private serviceOnRequestError(jqXHR: JQueryXHR): void {
            this.serviceOnRequestEnd(jqXHR);
            if ((<any>jqXHR).duracellkoErrorHandled) {
                return;
            }

            var errorMessage = jqXHR.responseText;
            if (errorMessage != null && errorMessage != "") {
                errorMessage = errorMessage.split("\n", 1)[0];
                this.messageBoxService.show(errorMessage, "Error");
            }
            else if (jqXHR.status != 0) {
                this.messageBoxService.show("Service is temporarily not available. Please, try again later.", "Error");
            }
            else {
                this.messageBoxService.show("Connection failed. Please, check your internet connection.", "Error");
            }
        }

        private JQueryAjaxOnBeforeSend(jqXHR: JQueryXHR, settings: JQueryAjaxSettings): boolean {
            this.serviceOnRequestBegin(jqXHR);
            return true;
        }

        private JQueryAjaxOnComplete(jqXHR: JQueryXHR, textStatus: string): void {
            this.serviceOnRequestEnd(jqXHR);
        }
    }

    export class CreateTeamController {
        private service: PlanningPokerService;
        private viewModel: CreateTeamViewModel = null;
        private view: JQuery = null;
        private viewTemplate: JQuery = null;

        public onTeamCreated: (scrumTeam: ScrumTeam, userName: string) => void = null;

        constructor(service: PlanningPokerService) {
            if (service == null) {
                throw new Exception("service");
            }

            this.service = service;
        }

        public initialize(view: JQuery): void {
            if (view == null) {
                throw new Exception("view");
            }

            if (this.viewTemplate == null) {
                this.viewTemplate = view.clone();
            }

            this.viewModel = new CreateTeamViewModel(this.service);
            this.viewModel.onTeamCreated = (scrumTeam, userName) => this.viewModelOnTeamCreated(scrumTeam, userName);
            this.view = this.viewTemplate.clone().replaceAll(view);
            ko.applyBindings(this.viewModel, this.view.get(0));
        }

        public dispose(): void {
            if (this.view != null) {
                ko.cleanNode(this.view.get(0));
                this.view = null;
            }
            if (this.viewModel != null) {
                this.viewModel.onTeamCreated = null;
                this.viewModel.dispose();
                this.viewModel = null;
            }
        }

        public show(): void {
            this.view.slideDown();
        }

        public hide(): void {
            if (this.view != null) {
                this.view.slideUp();
            }
        }

        private viewModelOnTeamCreated(scrumTeam: ScrumTeam, userName: string): void {
            if (this.onTeamCreated != null) {
                this.onTeamCreated(scrumTeam, userName);
            }
        }
    }

    export class CreateTeamViewModel {
        private service: PlanningPokerService;
        public teamName = ko.observable("");
        public scrumMasterName = ko.observable("");
        public isTeamNameValid = ko.pureComputed(this._isTeamNameValid, this);
        public isScrumMasterNameValid = ko.pureComputed(this._isScrumMasterNameValid, this);

        public createTeamCommand = () => this.createTeamCommandHandler();
        public onTeamCreated: (scrumTeam: ScrumTeam, userName: string) => void = null;

        private wasValidated = ko.observable(false);

        constructor(service: PlanningPokerService) {
            if (service == null) {
                throw new Exception("service");
            }

            this.service = service;
        }

        public dispose(): void {
            this.isTeamNameValid.dispose();
            this.isScrumMasterNameValid.dispose();
        }

        private createTeamCommandHandler(): void {
            if (this.validate()) {
                this.service.createTeam(this.teamName(), this.scrumMasterName()).done(sm => this.onCreateTeamDone(sm));
            }
        }

        private onCreateTeamDone(scrumTeam: ScrumTeam): void {
            if (this.onTeamCreated != null) {
                this.onTeamCreated(scrumTeam, this.scrumMasterName());
            }
        }

        private validate(): boolean {
            this.wasValidated(true);
            return this.isTeamNameValid() && this.isScrumMasterNameValid();
        }

        private _isTeamNameValid(): boolean {
            return !this.wasValidated() || (this.teamName() != null && this.teamName() != "");
        }

        private _isScrumMasterNameValid(): boolean {
            return !this.wasValidated() || (this.scrumMasterName() != null && this.scrumMasterName() != "");
        }
    }

    export class JoinTeamController {
        private service: PlanningPokerService;
        private messageBoxService: MessageBoxService;
        private userManager: UserManager;
        private viewModel: JoinTeamViewModel = null;
        private view: JQuery = null;
        private viewTemplate: JQuery = null;

        public onTeamJoined: (scrumTeam: ScrumTeam, userName: string) => void = null;
        public onTeamReconnected: (teamResult: ReconnectTeamResult, userName: string) => void = null;

        constructor(service: PlanningPokerService, messageBoxService: MessageBoxService, userManager: UserManager) {
            if (service == null) {
                throw new Exception("service");
            }
            if (messageBoxService == null) {
                throw new Exception("messageBoxService");
            }
            if (userManager == null) {
                throw new Exception("userManager");
            }

            this.service = service;
            this.messageBoxService = messageBoxService;
            this.userManager = userManager;
        }

        public initialize(view: JQuery): void {
            if (view == null) {
                throw new Exception("view");
            }

            if (this.viewTemplate == null) {
                this.viewTemplate = view.clone();
            }

            this.viewModel = new JoinTeamViewModel(this.service, this.messageBoxService, this.userManager);
            this.viewModel.onTeamJoined = (scrumTeam, userName) => this.viewModelOnTeamJoined(scrumTeam, userName);
            this.viewModel.onTeamReconnected = (teamResult, userName) => this.viewModelOnTeamReconnected(teamResult, userName);
            this.view = this.viewTemplate.clone().replaceAll(view);
            ko.applyBindings(this.viewModel, this.view.get(0));

            this.viewModel.initialize();
        }

        public dispose(): void {
            if (this.view != null) {
                ko.cleanNode(this.view.get(0));
                this.view = null;
            }
            if (this.viewModel != null) {
                this.viewModel.onTeamJoined = null;
                this.viewModel.onTeamReconnected = null;
                this.viewModel.dispose();
                this.viewModel = null;
            }
        }

        public show(): void {
            this.view.slideDown();
        }

        public hide(): void {
            if (this.view != null) {
                this.view.slideUp();
            }
        }

        private viewModelOnTeamJoined(scrumTeam: ScrumTeam, userName: string): void {
            if (this.onTeamJoined != null) {
                this.onTeamJoined(scrumTeam, userName);
            }
        }

        private viewModelOnTeamReconnected(teamResult: ReconnectTeamResult, userName: string): void {
            if (this.onTeamReconnected != null) {
                this.onTeamReconnected(teamResult, userName);
            }
        }
    }

    export class JoinTeamViewModel {
        private service: PlanningPokerService;
        private messageBoxService: MessageBoxService;
        private userManager: UserManager;
        public teamName = ko.observable("");
        public memberName = ko.observable("");
        public asObserver = ko.observable(false);
        public isTeamNameValid = ko.pureComputed(this._isTeamNameValid, this);
        public isMemberNameValid = ko.pureComputed(this._isMemberNameValid, this);

        public joinTeamCommand = () => this.joinTeamCommandHandler();
        public onTeamJoined: (scrumTeam: ScrumTeam, userName: string) => void = null;
        public onTeamReconnected: (teamResult: ReconnectTeamResult, userName: string) => void = null;

        private wasValidated = ko.observable(false);

        constructor(service: PlanningPokerService, messageBoxService: MessageBoxService, userManager: UserManager) {
            if (service == null) {
                throw new Exception("service");
            }
            if (messageBoxService == null) {
                throw new Exception("messageBoxService");
            }
            if (userManager == null) {
                throw new Exception("userManager");
            }

            this.service = service;
            this.messageBoxService = messageBoxService;
            this.userManager = userManager;
        }

        public initialize(): void {
            var configTeamName = this.userManager.teamName;
            var configUserName = this.userManager.userName;

            if (configTeamName != null && configTeamName != "") {
                this.teamName(configTeamName);

                if (configUserName != null && configUserName != "") {
                    this.memberName(configUserName);

                    var message = "You were disconnected. Do you want to reconnect to team '" + configTeamName + "' as user '" + configUserName + "'?";
                    this.messageBoxService.show(message, "Reconnect", "Reconnect",() => this.reconnectCommandHandler());
                }
            }
        }

        public dispose(): void {
            this.isTeamNameValid.dispose();
            this.isMemberNameValid.dispose();
        }

        private joinTeamCommandHandler(): void {
            if (this.validate()) {
                this.service.joinTeam(this.teamName(), this.memberName(), this.asObserver()).done(sm => this.onJoinedTeamDone(sm))
                    .fail((e: any) => this.onJoinedTeamFail(e));
            }
        }

        private onJoinedTeamDone(scrumTeam: ScrumTeam): void {
            if (this.onTeamJoined != null) {
                this.onTeamJoined(scrumTeam, this.memberName());
            }
        }

        private onJoinedTeamFail(jqXHR: JQueryXHR): void {
            var errorMessage = jqXHR.responseText;
            if (errorMessage != null && errorMessage != "") {
                if (errorMessage.indexOf("Member or observer named") >= 0 &&
                    errorMessage.indexOf("already exists in the team.") >= 0) {
                    errorMessage = errorMessage.split("\n", 1)[0];
                    errorMessage += " Do you want to reconnect?";
                    this.messageBoxService.show(errorMessage, "Reconnect", "Reconnect", () => this.reconnectCommandHandler());
                    (<any>jqXHR).duracellkoErrorHandled = true;
                }
            }
        }
        
        private validate(): boolean {
            this.wasValidated(true);
            return this.isTeamNameValid() && this.isMemberNameValid();
        }

        private _isTeamNameValid(): boolean {
            return !this.wasValidated() || (this.teamName() != null && this.teamName() != "");
        }

        private _isMemberNameValid(): boolean {
            return !this.wasValidated() || (this.memberName() != null && this.memberName() != "");
        }

        private reconnectCommandHandler(): boolean {
            this.service.reconnectTeam(this.teamName(), this.memberName()).done(rtr => this.onReconnectedTeamDone(rtr));
            return true;
        }

        private onReconnectedTeamDone(teamResult: ReconnectTeamResult): void {
            if (this.onTeamReconnected != null) {
                this.onTeamReconnected(teamResult, this.memberName());
            }
        }
    }

    export class PlanningPokerDeskController {
        private service: PlanningPokerService;
        private userManager: UserManager;
        private scrumTeamController: ScrumTeamController;
        private userInfoController: UserInfoController;
        private messageController: PlanningPokerMessageController = null;

        public onTeamDisconnected: (teamName: string, userName: string) => void = null;

        constructor(service: PlanningPokerService, userManager: UserManager) {
            if (service == null) {
                throw new Exception("service");
            }
            if (userManager == null) {
                throw new Exception("userManager");
            }

            this.service = service;
            this.userManager = userManager;
            this.scrumTeamController = new ScrumTeamController(this.service);
            this.userInfoController = new UserInfoController(this.service);
            this.userInfoController.onTeamDisconnected = (teamName, userName) => this.userInfoControllerOnTeamDisconnected(teamName, userName);
        }

        public initialize(scrumTeam: ScrumTeam, userName: string, container: JQuery, lastMessageId: number = null, selectedEstimation: Estimation = null): void {
            if (scrumTeam == null) {
                throw new Exception("scrumTeam");
            }
            if (userName == null || userName == "") {
                throw new Exception("userName");
            }

            this.messageController = new PlanningPokerMessageController(this.service, scrumTeam.name, userName, this.userManager);
            this.messageController.onMessageReceived = m => this.processMessage(m);
            this.messageController.onTeamDisconnected = (teamName, userName) => this.messageControllerOnTeamDisconnected(teamName, userName);
            if (lastMessageId != null) {
                this.messageController.lastMessageId = lastMessageId;
            }

            var pokerDeskPanel = $("#pokerDeskPanel", container);
            var membersPanel = $("#membersPanel", container);
            this.scrumTeamController.initialize(scrumTeam, userName, pokerDeskPanel, membersPanel, selectedEstimation);

            var userInfoPanel = $("#userInfoPanel", container);
            this.userInfoController.initialize(scrumTeam.name, userName, userInfoPanel);

            this.messageController.start();
        }

        public dispose(): void {
            if (this.messageController != null) {
                this.messageController.stop();
                this.messageController.onMessageReceived = null;
                this.messageController.onTeamDisconnected = null;
                this.messageController = null;
            }

            this.userInfoController.dispose();
            this.scrumTeamController.dispose();
        }

        public show(): void {
            this.scrumTeamController.show();
            this.userInfoController.show();
        }

        public hide(): void {
            this.scrumTeamController.hide();
            this.userInfoController.hide();
        }

        private processMessage(message: Message): boolean {
            if (this.scrumTeamController.viewModel == null) {
                return false;
            }

            var vm = this.scrumTeamController.viewModel;
            switch (message.type) {
                case MessageType.MemberJoined:
                    var memberMessage = <MemberMessage>message;
                    vm.joinMember(memberMessage.member.name, memberMessage.member.type == "Observer");
                    break;
                case MessageType.MemberDisconnected:
                    var memberMessage = <MemberMessage>message;
                    vm.disconnectMember(memberMessage.member.name);
                    break;
                case MessageType.EstimationStarted:
                    vm.startEstimation();
                    break;
                case MessageType.EstimationEnded:
                    var estimationResultMessage = <EstimationResultMessage>message;
                    vm.endEstimation(estimationResultMessage.estimationResult);
                    break;
                case MessageType.EstimationCanceled:
                    vm.cancelEstimation();
                    break;
                case MessageType.MemberEstimated:
                    var memberMessage = <MemberMessage>message;
                    vm.memberEstimated(memberMessage.member.name);
                    break;
            }

            return true;
        }

        private userInfoControllerOnTeamDisconnected(teamName: string, userName: string): void {
            if (this.onTeamDisconnected != null) {
                this.onTeamDisconnected(teamName, userName);
            }
        }

        private messageControllerOnTeamDisconnected(teamName: string, userName: string): void {
            if (this.onTeamDisconnected != null) {
                this.onTeamDisconnected(teamName, userName);
            }
        }
    }

    export class PlanningPokerMessageController {
        private service: PlanningPokerService;
        private userManager: UserManager;
        private teamName: string;
        private userName: string;
        private isActive: boolean = false;
        private _lastMessageId: number = 0;
        private errorCount: number = 0;
        private getMessagesPromise: PromiseWithAbort<Message[]> = null;
        private timer: number = null;

        public onMessageReceived: (message: Message) => boolean = null;
        public onTeamDisconnected: (teamName: string, userName: string) => void = null;

        constructor(service: PlanningPokerService, teamName: string, userName: string, userManager: UserManager) {
            if (service == null) {
                throw new Exception("service");
            }
            if (teamName == null || teamName == "") {
                throw new Exception("teamName");
            }
            if (userName == null || userName == "") {
                throw new Exception("userName");
            }
            if (userManager == null) {
                throw new Exception("userManager");
            }

            this.service = service;
            this.teamName = teamName;
            this.userName = userName;
            this.userManager = userManager;
        }

        public get lastMessageId(): number {
            return this._lastMessageId;
        }

        public set lastMessageId(value: number) {
            this._lastMessageId = value;
        }

        public start(): void {
            this.isActive = true;
            this.errorCount = 0;
            this.userManager.saveUser(this.teamName, this.userName);
            this.getMessages();
        }

        public stop(): void {
            this.isActive = false;

            if (this.timer != null) {
                window.clearTimeout(this.timer);
            }

            if (this.getMessagesPromise != null) {
                this.getMessagesPromise.abort();
            }

            this.userManager.saveUser(this.teamName, null);
        }

        private getMessages(): void {
            if (this.isActive) {
                this.getMessagesPromise = this.service.getMessages(this.teamName, this.userName, this.lastMessageId);
                this.getMessagesPromise.promise.done(m => this.onGetMessagesFinished(m)).fail(() => this.onGetMessagesError());
            }
        }

        private onGetMessagesFinished(messages: Message[]): void {
            if (this.isActive) {
                if (messages != null) {
                    this.errorCount = 0;
                    this.userManager.saveUser(this.teamName, this.userName);

                    Enumerable.From(messages).ForEach(m => {
                        if (this.onMessageReceived != null) {
                            if (this.onMessageReceived(m)) {
                                this.lastMessageId = m.id;
                            }
                        }
                    });
                }

                this.getMessages();
            }
        }

        private onGetMessagesError(): void {
            if (this.isActive) {
                if (this.errorCount < 24) {
                    this.errorCount++;
                    this.timer = window.setTimeout(() => {
                        this.timer = null;
                        this.getMessages();
                    }, 5000);
                }
                else {
                    if (this.onTeamDisconnected != null) {
                        this.onTeamDisconnected(this.teamName, this.userName);
                    }
                }
            }
        }
    }

    export class ScrumTeamController {
        private service: PlanningPokerService;
        private _viewModel: ScrumTeamViewModel = null;
        private pokerDeskView: JQuery = null;
        private pokerDeskViewTemplate: JQuery = null;
        private membersView: JQuery = null;
        private membersViewTemplate: JQuery = null;

        constructor(service: PlanningPokerService) {
            if (service == null) {
                throw new Exception("service");
            }

            this.service = service;
        }

        public get viewModel(): ScrumTeamViewModel {
            return this._viewModel;
        }

        public initialize(scrumTeam: ScrumTeam, userName: string, pokerDeskView: JQuery, membersView: JQuery, selectedEstimation: Estimation = null): void {
            if (scrumTeam == null) {
                throw new Exception("scrumTeam");
            }
            if (userName == null || userName == "") {
                throw new Exception("userName");
            }

            if (this.pokerDeskViewTemplate == null) {
                this.pokerDeskViewTemplate = pokerDeskView.clone();
            }
            if (this.membersViewTemplate == null) {
                this.membersViewTemplate = membersView.clone();
            }

            this._viewModel = new ScrumTeamViewModel(this.service, scrumTeam, userName, selectedEstimation);
            this.pokerDeskView = this.pokerDeskViewTemplate.clone().replaceAll(pokerDeskView);
            this.membersView = this.membersViewTemplate.clone().replaceAll(membersView);
            if (this.pokerDeskView != null) {
                ko.applyBindings(this.viewModel, this.pokerDeskView.get(0));
            }
            if (this.membersView != null) {
                ko.applyBindings(this.viewModel, this.membersView.get(0));
            }
        }

        public dispose(): void {
            if (this.pokerDeskView != null) {
                ko.cleanNode(this.pokerDeskView.get(0));
                this.pokerDeskView = null;
            }
            if (this.membersView != null) {
                ko.cleanNode(this.membersView.get(0));
                this.membersView = null;
            }
            if (this._viewModel != null) {
                this._viewModel.dispose();
                this._viewModel = null;
            }
        }

        public show(): void {
            if (this.pokerDeskView != null) {
                this.pokerDeskView.slideDown();
            }
            if (this.membersView != null) {
                this.membersView.slideDown();
            }
        }

        public hide(): void {
            if (this.pokerDeskView != null) {
                this.pokerDeskView.slideUp();
            }
            if (this.membersView != null) {
                this.membersView.slideUp();
            }
        }
    }

    export class ScrumTeamViewModel {
        private service: PlanningPokerService;
        private userName: string;
        public name = ko.observable<string>(null);
        public scrumMaster = ko.observable<TeamMemberViewModel>(null);
        public members = ko.observableArray<TeamMemberViewModel>();
        public observers = ko.observableArray<TeamMemberViewModel>();
        public state = ko.observable(TeamState.Initial);
        public availableEstimations = ko.observableArray<EstimationViewModel>();
        public selectedEstimation = ko.observable<EstimationViewModel>(null);
        public estimationResultItems = ko.observableArray<EstimationResultItemViewModel>();
        public isJoinedInEstimation = ko.observable(false);
        public isScrumMaster = ko.pureComputed(this._isScrumMaster, this);
        public canStartEstimation = ko.pureComputed(this._canStartEstimation, this);
        public canCancelEstimation = ko.pureComputed(this._canCancelEstimation, this);
        public canSelectEstimation = ko.pureComputed(this._canSelectEstimation, this);
        public hasEstimationResult = ko.pureComputed(this._hasEstimationResult, this);

        public startEstimationCommand = () => this.startEstimationCommandHandler();
        public cancelEstimationCommand = () => this.cancelEstimationCommandHandler();
        public selectEstimationCommand: (estimation: EstimationViewModel) => void = estimation => this.selectEstimationCommandHandler(estimation);
        public kickoffMemberCommand: (member: TeamMemberViewModel) => void = member => this.kickoffMemberCommandHandler(member);

        constructor(service: PlanningPokerService, scrumTeam: ScrumTeam, userName: string, selectedEstimation: Estimation = null) {
            if (service == null) {
                throw new Exception("service");
            }
            if (scrumTeam == null) {
                throw new Exception("scrumTeam");
            }
            if (userName == null || userName == "") {
                throw new Exception("userName");
            }

            this.service = service;
            this.userName = userName;
            this.initialiazeFromScrumTeam(scrumTeam, selectedEstimation);
        }

        public joinMember(memberName: string, asObserver: boolean): void {
            if (memberName == null || memberName == "") {
                throw new Exception("memberName");
            }

            var tm = new TeamMemberViewModel(memberName);
            if (asObserver) {
                this.observers.push(tm);
            }
            else {
                this.members.push(tm);
            }
        }

        public disconnectMember(memberName: string): void {
            if (memberName == null || memberName == "") {
                throw new Exception("memberName");
            }

            if (this.scrumMaster() != null && this.scrumMaster().name() == memberName) {
                this.scrumMaster(null);
            }
            else {
                this.members.remove(m => m.name() == memberName);
                this.observers.remove(m => m.name() == memberName);
            }
        }

        public startEstimation(): void {
            this.selectedEstimation(null);
            this.estimationResultItems.removeAll();
            if (this.isMember()) {
                this.isJoinedInEstimation(true);
            }
            this.state(TeamState.EstimationInProgress);
        }

        public cancelEstimation(): void {
            this.state(TeamState.EstimationCanceled);
            this.isJoinedInEstimation(false);
        }

        public endEstimation(estimationResults: EstimationResultItem[]): void {
            if (estimationResults == null) {
                throw new Exception("estimationResults");
            }

            this.state(TeamState.EstimationFinished);
            this.isJoinedInEstimation(false);
            this.estimationResultItems.removeAll();

            var estimationViewModels = Enumerable.From(estimationResults).Select(er => this.createEstimationResultItemViewModel(er));
            estimationViewModels = Enumerable.From(estimationViewModels.ToArray());

            var orderedResults = estimationViewModels.OrderByDescending(e => {
                var estimation = e.estimation();
                if (estimation == null) {
                    return -1;
                }
                var value = estimation.value();
                return estimationViewModels.Count(i => i.estimation() != null && i.estimation().value() == value);
            });

            orderedResults = orderedResults.ThenByDescending(e => e.estimation() != null ? e.estimation() : Estimation.positiveInfinity - 1);
            orderedResults.ForEach(e => this.estimationResultItems.push(e));
        }

        public memberEstimated(memberName: string): void {
            if (memberName == null || memberName == "") {
                throw new Exception("memberName");
            }

            this.estimationResultItems.push(new EstimationResultItemViewModel(memberName));
        }

        public dispose(): void {
            this.isScrumMaster.dispose();
            this.canStartEstimation.dispose();
            this.canCancelEstimation.dispose();
            this.canSelectEstimation.dispose();
            this.hasEstimationResult.dispose();
        }

        private initialiazeFromScrumTeam(scrumTeam: ScrumTeam, selectedEstimation: Estimation = null): void {
            this.name(scrumTeam.name);

            if (scrumTeam.scrumMaster != null) {
                this.scrumMaster(new TeamMemberViewModel(scrumTeam.scrumMaster.name));
            }
            if (scrumTeam.members != null) {
                Enumerable.From(scrumTeam.members).Where(m => m.type != "ScrumMaster")
                    .ForEach(m => this.members.push(new TeamMemberViewModel(m.name)));
            }
            if (scrumTeam.observers != null) {
                Enumerable.From(scrumTeam.observers)
                    .ForEach(o => this.observers.push(new TeamMemberViewModel(o.name)));
            }

            this.state(scrumTeam.state);

            if (scrumTeam.avilableEstimations != null) {
                Enumerable.From(scrumTeam.avilableEstimations)
                    .ForEach(e => this.availableEstimations.push(new EstimationViewModel(e.value)));
            }

            if (scrumTeam.estimationResult != null) {
                Enumerable.From(scrumTeam.estimationResult)
                    .ForEach(er => this.estimationResultItems.push(this.createEstimationResultItemViewModel(er)));
            }
            else if (scrumTeam.estimationParticipants != null) {
                Enumerable.From(scrumTeam.estimationParticipants).Where(ep => ep.estimated)
                    .ForEach(ep => this.estimationResultItems.push(new EstimationResultItemViewModel(ep.memberName)));
            }

            if (selectedEstimation != null) {
                var estimation = new EstimationViewModel(selectedEstimation.value);
                this.selectedEstimation(estimation);
            }

            var isJoined = false;
            if (scrumTeam.estimationParticipants != null) {
                isJoined = Enumerable.From(scrumTeam.estimationParticipants).Any(ep => ep.memberName == this.userName);
            }
            this.isJoinedInEstimation(isJoined);
        }

        private _isScrumMaster(): boolean {
            return this.scrumMaster() != null && this.scrumMaster().name() == this.userName;
        }

        private _canStartEstimation(): boolean {
            return this.isScrumMaster() && this.state() != TeamState.EstimationInProgress;
        }

        private _canCancelEstimation(): boolean {
            return this.isScrumMaster() && this.state() == TeamState.EstimationInProgress;
        }

        private _canSelectEstimation(): boolean {
            return this.state() == TeamState.EstimationInProgress && this.isJoinedInEstimation() && this.selectedEstimation() == null;
        }

        private _hasEstimationResult(): boolean {
            return this.estimationResultItems().length > 0;
        }

        private startEstimationCommandHandler() {
            if (this.canStartEstimation()) {
                this.service.startEstimation(this.name());
            }
        }

        private cancelEstimationCommandHandler() {
            if (this.canCancelEstimation()) {
                this.service.cancelEstimation(this.name());
            }
        }

        private selectEstimationCommandHandler(estimation: EstimationViewModel): void {
            if (estimation != null) {
                this.service.submitEstimation(this.name(), this.userName, estimation.value()).done(() => this.submitEstimationDone(estimation));
            }
        }

        private submitEstimationDone(estimation: EstimationViewModel) {
            this.selectedEstimation(estimation);
        }

        private kickoffMemberCommandHandler(member: TeamMemberViewModel): void {
            if (member != null && this.isScrumMaster()) {
                this.service.disconnectTeam(this.name(), member.name());
            }
        }

        private isMember(): boolean {
            if (this.isScrumMaster()) {
                return true;
            }

            return Enumerable.From(this.members()).Any(m => m.name() == this.userName);
        }

        private createEstimationResultItemViewModel(estimationResult: EstimationResultItem): EstimationResultItemViewModel {
            var result = new EstimationResultItemViewModel(estimationResult.member.name);
            if (estimationResult.estimation != null) {
                result.estimation(new EstimationViewModel(estimationResult.estimation.value));
            }
            return result;
        }
    }

    export class TeamMemberViewModel {
        public name = ko.observable<string>(null);

        constructor(name: string) {
            if (name == null || name == "") {
                throw new Exception("name");
            }

            this.name(name);
        }
    }

    export class EstimationViewModel {
        public value = ko.observable<number>(null);
        public caption = ko.pureComputed(this._caption, this);

        constructor(value: number) {
            this.value(value);
        }

        private _caption(): string {
            var v = this.value();
            if (v === null) {
                return "?";
            }
            else if (v == Estimation.positiveInfinity) {
                return "\u221E";
            }
            else if (v == 0.5) {
                return "\u00BD";
            }
            else {
                return v.toString();
            }
        }
    }

    export class EstimationResultItemViewModel {
        public member = ko.observable<TeamMemberViewModel>(null);
        public estimation = ko.observable<EstimationViewModel>(null);

        constructor(memberName: string) {
            if (memberName == null || memberName == "") {
                throw new Exception("memberName");
            }

            this.member(new TeamMemberViewModel(memberName));
        }
    }

    export class UserInfoController {
        private service: PlanningPokerService;
        private viewModel: UserInfoViewModel = null;
        private view: JQuery = null;
        private viewTemplate: JQuery = null;

        public onTeamDisconnected: (teamName: string, userName: string) => void = null;

        constructor(service: PlanningPokerService) {
            if (service == null) {
                throw new Exception("service");
            }

            this.service = service;
        }

        public initialize(teamName: string, userName: string, view: JQuery): void {
            if (view == null) {
                throw new Exception("view");
            }

            if (this.viewTemplate == null) {
                this.viewTemplate = view.clone();
            }

            this.viewModel = new UserInfoViewModel(this.service, teamName, userName);
            this.viewModel.onTeamDisconnected = (teamName, userName) => this.viewModelOnTeamDisconnected(teamName, userName);
            this.view = this.viewTemplate.clone().replaceAll(view);
            ko.applyBindings(this.viewModel, this.view.get(0));
        }

        public dispose(): void {
            if (this.view != null) {
                ko.cleanNode(this.view.get(0));
                this.view = null;
            }
            if (this.viewModel != null) {
                this.viewModel.onTeamDisconnected = null;
                this.viewModel.dispose();
                this.viewModel = null;
            }
        }

        public show(): void {
            this.view.show();
        }

        public hide(): void {
            if (this.view != null) {
                this.view.hide();
            }
        }

        private viewModelOnTeamDisconnected(teamName: string, userName: string): void {
            if (this.onTeamDisconnected != null) {
                this.onTeamDisconnected(teamName, userName);
            }
        }
    }

    export class UserInfoViewModel {
        private service: PlanningPokerService;
        private teamName: string;
        public userName = ko.observable<string>(null);

        public disconnectCommand = () => this.disconnectCommandHandler();
        public onTeamDisconnected: (teamName: string, userName: string) => void = null;

        constructor(service: PlanningPokerService, teamName: string, userName: string) {
            if (service == null) {
                throw new Exception("service");
            }
            if (teamName == null || teamName == "") {
                throw new Exception("teamName");
            }
            if (userName == null || userName == "") {
                throw new Exception("userName");
            }

            this.service = service;
            this.teamName = teamName
            this.userName(userName);
        }

        public dispose() {
        }

        private disconnectCommandHandler(): void {
            this.service.disconnectTeam(this.teamName, this.userName())
                .done(() => this.onDisconnectTeamDone());
        }

        private onDisconnectTeamDone(): void {
            if (this.onTeamDisconnected != null) {
                this.onTeamDisconnected(this.teamName, this.userName());
            }
        }
    }

    class PromiseWithAbort<T> {
        public promise: JQueryPromise<T> = null;
        public abort = () => { };
    }

    class UserManager {
        private static cookieExpiration: number = 900000;
        private static cookiePrefix: string = "TEAM_";

        public get teamName(): string {
            var teamUser = this.parseUrlHash();
            return teamUser != null && teamUser.team != "" ? teamUser.team : null;
        }

        public get userName(): string {
            var teamUser = this.parseUrlHash();
            if (teamUser != null && teamUser.team != null && teamUser.team != "" && teamUser.user != null && teamUser.user != "") {
                var userCookie = this.getCookie(teamUser.team);
                if (teamUser.user == userCookie) {
                    return teamUser.user;
                }
            }

            return null;
        }

        public saveUser(teamName: string, userName: string = null): void {
            var userData = {
                team: teamName,
                user: userName
            };
            window.location.replace("#" + $.param(userData));
            this.setCookie(teamName, userName);
        }

        private static parseCookiePair(pair: string): TeamUser {
            var nameValue = pair.split("=", 2);
            if (nameValue.length < 2) {
                return null;
            }

            var name = nameValue[0];
            try {
                name = decodeURIComponent(name.trim());
            }
            catch (ex) {
                name = null;
            }

            var value = nameValue[1];
            try {
                value = decodeURIComponent(value.trim());
            }
            catch (ex) {
                value = null;
            }

            if (name == null || name.length <= UserManager.cookiePrefix.length ||
                name.substr(0, UserManager.cookiePrefix.length) != UserManager.cookiePrefix) {
                return null;
            }

            return <TeamUser>{
                team: name.substr(UserManager.cookiePrefix.length),
                user: value
            }
        }

        private static parseUrlHashPair(pair: string): string[]{
            var name: string = null;
            var value: string = null;

            var nameValue = pair.split("=", 2);
            if (nameValue.length < 2) {
                return [name, value];
            }

            try {
                name = decodeURIComponent(nameValue[0]);
            }
            catch (ex) {
                name = null;
            }

            try {
                value = decodeURIComponent(nameValue[1]);
            }
            catch (ex) {
                value = null;
            }

            return [name, value];
        }

        private parseUrlHash(): TeamUser {
            var url = window.location.toString();
            var urlParts = url.split("#", 2);
            if (urlParts.length < 2) {
                return null;
            }

            var hash = urlParts[1];
            if (hash == null || hash.length == 0) {
                return null;
            }

            if (hash[0] == "#") {
                hash = hash.substr(1);
            }

            if (hash == "") {
                return null;
            }

            var result = new TeamUser();
            var hashComponents = Enumerable.From(hash.split("&"));
            var hashPairs = hashComponents.Select(c => UserManager.parseUrlHashPair(c));
            hashPairs.ForEach(p => {
                if (p[0] == "team") {
                    result.team = p[1];
                }
                else if (p[0] == "user") {
                    result.user = p[1];
                }
            });

            return result.team != null && result.team != "" ? result : null;
        }

        private getCookie(teamName: string): string {
            var cookie = document.cookie;
            var cookiePairs = Enumerable.From(cookie.split(";"));
            var teamUserList = cookiePairs.Select(p => UserManager.parseCookiePair(p)).Where(tu => tu != null);
            var teamUser = teamUserList.FirstOrDefault(null, tu => tu.team == teamName);
            return teamUser != null ? teamUser.user : null;
        }

        private setCookie(teamName: string, userName: string): void {
            var cookie = encodeURIComponent(UserManager.cookiePrefix + teamName) + "=";
            if (userName != null && userName != "") {
                cookie += encodeURIComponent(userName);
            }

            var expiration = new Date(Date.now() + UserManager.cookieExpiration);
            cookie += "; expires=" + expiration.toUTCString();

            document.cookie = cookie;
        }
    }

    class TeamUser {
        public team: string = null;
        public user: string = null;
    }

    class PlanningPokerService {
        private serviceUrl: string;

        public onRequestBegin: (jqXHR: JQueryXHR) => void = null;
        public onRequestEnd: (jqXHR: JQueryXHR) => void = null;
        public onRequestError: (jqXHR: JQueryXHR) => void = null;

        constructor(serviceUrl: string) {
            if (serviceUrl == null || serviceUrl == "") {
                throw new Exception("serviceUrl");
            }

            this.serviceUrl = serviceUrl;
        }

        public createTeam(teamName: string, scrumMasterName: string): JQueryPromise<ScrumTeam> {
            var createTeamData = {
                teamName: teamName,
                scrumMasterName: scrumMasterName
            };
            var url = this.serviceUrl + "/CreateTeam?" + $.param(createTeamData);
            var settings = this.createAjaxSettings();
            var promise: JQueryPromise<any> = $.ajax(url, settings);
            return promise.then<ScrumTeam>(this.convertScrumTeam);
        }

        public joinTeam(teamName: string, memberName: string, asObserver: boolean): JQueryPromise<ScrumTeam> {
            var joinTeamData = {
                teamName: teamName,
                memberName: memberName,
                asObserver: asObserver
            };
            var url = this.serviceUrl + "/JoinTeam?" + $.param(joinTeamData);
            var settings = this.createAjaxSettings();
            var promise: JQueryPromise<any> = $.ajax(url, settings);
            return promise.then<ScrumTeam>(this.convertScrumTeam);
        }

        public reconnectTeam(teamName: string, memberName: string): JQueryPromise<ReconnectTeamResult> {
            var joinTeamData = {
                teamName: teamName,
                memberName: memberName,
            };
            var url = this.serviceUrl + "/ReconnectTeam?" + $.param(joinTeamData);
            var settings = this.createAjaxSettings();
            var promise: JQueryPromise<any> = $.ajax(url, settings);
            return promise.then<ReconnectTeamResult>(this.convertReconnectTeamResult);
        }

        public disconnectTeam(teamName: string, memberName: string): JQueryPromise<void> {
            var disconnectTeamData = {
                teamName: teamName,
                memberName: memberName
            };
            var url = this.serviceUrl + "/DisconnectTeam?" + $.param(disconnectTeamData);
            var settings = this.createAjaxSettings(true);
            var promise: JQueryPromise<any> = $.ajax(url, settings);
            return promise.then<void>(this.convertEmpty);
        }

        public startEstimation(teamName: string): JQueryPromise<void> {
            var startEstimationData = {
                teamName: teamName
            }
            var url = this.serviceUrl + "/StartEstimation?" + $.param(startEstimationData);
            var settings = this.createAjaxSettings(true);
            var promise: JQueryPromise<any> = $.ajax(url, settings);
            return promise.then<void>(this.convertEmpty);
        }

        public cancelEstimation(teamName: string): JQueryPromise<void> {
            var cancelEstimationData = {
                teamName: teamName
            }
            var url = this.serviceUrl + "/CancelEstimation?" + $.param(cancelEstimationData);
            var settings = this.createAjaxSettings(true);
            var promise: JQueryPromise<any> = $.ajax(url, settings);
            return promise.then<void>(this.convertEmpty);
        }

        public submitEstimation(teamName: string, memberName: string, estimation: number): JQueryPromise<void> {
            var submitEstimationData = {
                teamName: teamName,
                memberName: memberName,
                estimation: estimation != null ? estimation : -1111111
            }
            var url = this.serviceUrl + "/SubmitEstimation?" + $.param(submitEstimationData);
            var settings = this.createAjaxSettings(true);
            var promise: JQueryPromise<any> = $.ajax(url, settings);
            return promise.then<void>(this.convertEmpty);
        }

        public getMessages(teamName: string, memberName: string, lastMessageId: number): PromiseWithAbort<Message[]> {
            var getMessagesData = {
                teamName: teamName,
                memberName: memberName,
                lastMessageId: lastMessageId
            }
            var url = this.serviceUrl + "/GetMessages?" + $.param(getMessagesData);
            var settings = this.createAjaxSettings();
            settings.timeout = 90000;
            settings.beforeSend = null;
            settings.complete = null;
            settings.error = null;

            var jqXHR = $.ajax(url, settings);
            var promise: JQueryPromise<any> = jqXHR;

            var result = new PromiseWithAbort<Message[]>();
            result.promise = promise.then<Message[]>(this.convertMessages);
            result.abort = () => jqXHR.abort();
            return result;
        }

        private createAjaxSettings(emptyResult: boolean = false): JQueryAjaxSettings {
            return <JQueryAjaxSettings>{
                dataType: emptyResult ? "text" : "json",
                timeout: 30000,
                beforeSend: (jqXHR: JQueryXHR, settings: JQueryAjaxSettings) => this.JQueryAjaxOnBeforeSend(jqXHR, settings),
                complete: (jqXHR: JQueryXHR, textStatus: string) => this.JQueryAjaxOnComplete(jqXHR, textStatus),
                error: (jqXHR: JQueryXHR, textStatus: string, errorThrown: string) => this.JQueryAjaxOnError(jqXHR, textStatus, errorThrown)
            };
        }

        private convertEmpty(data: any): void {
        }

        private convertScrumTeam(data: any): ScrumTeam {
            return <ScrumTeam>data;
        }

        private convertReconnectTeamResult(data: any): ReconnectTeamResult {
            return <ReconnectTeamResult>data;
        }

        private convertMessages(data: any): Message[]{
            return <Message[]>data;
        }

        private JQueryAjaxOnBeforeSend(jqXHR: JQueryXHR, settings: JQueryAjaxSettings): boolean {
            if (this.onRequestBegin != null) {
                this.onRequestBegin(jqXHR);
            }
            return true;
        }

        private JQueryAjaxOnComplete(jqXHR: JQueryXHR, textStatus: string): void {
            if (this.onRequestEnd != null) {
                this.onRequestEnd(jqXHR);
            }
        }

        private JQueryAjaxOnError(jqXHR: JQueryXHR, textStatus: string, errorThrown: string): void {
            if (this.onRequestError != null) {
                this.onRequestError(jqXHR);
            }
        }
    }
}