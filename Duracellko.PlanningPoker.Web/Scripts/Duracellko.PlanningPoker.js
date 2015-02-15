// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

/// <reference path="jquery-2.1.3.js" />
/// <reference path="jquery-ui-1.11.2.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="knockout-3.2.0.js" />
/// <reference path="jquery.blockUI.js" />
/// <reference path="JSLINQ.js" />

(function (window, undefined) {

    var TeamState = {
        initial: 0,
        estimationInProgress: 1,
        estimationFinished: 2,
        estimationCanceled: 3
    };

    var ScrumTeam = function (name, userName) {
        this._t = this;
        var self = this;
        this.name = ko.observable(name);
        this.userName = userName;
        this.scrumMaster = ko.observable(null);
        this.members = ko.observableArray();
        this.observers = ko.observableArray();
        this.state = ko.observable(TeamState.initial);
        this.availableEstimations = ko.observableArray();
        this.selectedEstimation = ko.observable(null);
        this.estimationResultItems = ko.observableArray();
        this.isJoinedInEstimation = ko.observable(false);
        this.isScrumMaster = ko.computed(function () { return this.scrumMaster() != null && this.scrumMaster().name() == this.userName }, this);
        this.canStartEstimation = ko.computed(function () { return this.isScrumMaster() && this.state() != TeamState.estimationInProgress; }, this);
        this.canCancelEstimation = ko.computed(function () { return this.isScrumMaster() && this.state() == TeamState.estimationInProgress; }, this);
        this.canSelectEstimation = ko.computed(
            function () {
                return this.state() == TeamState.estimationInProgress && this.isJoinedInEstimation() && this.selectedEstimation() == null;
            },
            this);
        this.hasEstimationResult = ko.computed(function () { return this.estimationResultItems().length > 0; }, this);
        this.selectEstimation = function (estimation) {
            self.serviceProvider.selectEstimation.call(self.serviceProvider, estimation);
        };
        this.kickoffMember = function (member) {
            self.serviceProvider.kickoffMember.call(self.serviceProvider, member, self);
        };
    };
    ScrumTeam.prototype = {
        userName: null,
        name: null,
        scrumMaster: null,
        members: null,
        observers: null,
        state: null,
        isScrumMaster: null,
        availableEstimations: null,
        selectedEstimation: null,
        estimationResult: null,
        isJoinedInEstimation: null,
        joinMember: function (memberName, asObserver) {
            var t = this._t;
            var tm = new TeamMember(memberName);
            if (asObserver) {
                t.observers.push(tm);
            }
            else {
                t.members.push(tm);
            }
        },
        disconnectMember: function (memberName) {
            var t = this._t;
            if (t.scrumMaster() != null && t.scrumMaster().name() == memberName) {
                t.scrumMaster(null);
            }
            else {
                t.members.remove(function (m) { return m.name() == memberName });
                t.observers.remove(function (m) { return m.name() == memberName });
            }
        },
        startEstimation: function () {
            var t = this._t;
            t.selectedEstimation(null);
            t.estimationResultItems.removeAll();
            if (t._isMember()) {
                t.isJoinedInEstimation(true);
            }
            t.state(TeamState.estimationInProgress);
        },
        cancelEstimation: function () {
            var t = this._t;
            t.state(TeamState.estimationCanceled);
            t.isJoinedInEstimation(false);
        },
        endEstimation: function (estimationResult) {
            var t = this._t;
            t.state(TeamState.estimationFinished);
            t.isJoinedInEstimation(false);
            t.estimationResultItems.removeAll();

            var orderedResults = JSLINQ(estimationResult).OrderByDescending(
                function (item) {
                    var estimation = item.estimation();
                    if (estimation == null) {
                        return -1;
                    }
                    var value = estimation.value();
                    return JSLINQ(estimationResult).Where(function (i) { return i.estimation() != null && i.estimation().value() == value; }).Count();
                });
            orderedResults = orderedResults.ToArray();

            for (var i = 0; i < orderedResults.length; i++) {
                t.estimationResultItems.push(orderedResults[i]);
            }
        },
        memberEstimated: function (memberName) {
            var t = this._t;
            t.estimationResultItems.push(new EstimationResultItem(memberName));
        },
        _isMember: function () {
            var t = this._t;
            if (t.isScrumMaster()) {
                return true;
            }
            for (var i = 0; i < t.members().length; i++) {
                if (t.members()[i].name() == t.userName) {
                    return true;
                }
            }
            return false;
        }
    };

    var TeamMember = function (name) {
        this._t = this;
        this.name = ko.observable(name);
    };
    TeamMember.prototype = {
        name: null
    };

    var Estimation = function (value) {
        this._t = this;
        this.value = ko.observable(value);
        this.caption = ko.computed(
            function () {
                var t = this._t;
                var v = t.value();
                if (v === null) {
                    return "?";
                }
                else if (v == -1111100) {
                    return "\u221E";
                }
                else if (v == 0.5) {
                    return "\u00BD";
                }
                else {
                    return v.toString();
                }
            },
            this);
    };
    Estimation.prototype = {
        value: null,
        caption: null
    }

    var EstimationResultItem = function (memberName) {
        this._t = this;
        this.member = ko.observable(new TeamMember(memberName));
        this.estimation = ko.observable(null);
    }
    EstimationResultItem.prototype = {
        member: null,
        estimation: null
    }

    var planningPoker = function (container, serviceUrl, templateUrl) {
        this._t = this;
        this._container = $(container);
        this._serviceUrl = serviceUrl;
        this._templateUrl = templateUrl;

        this._createTeamViewModel = null;
        this._joinTeamViewModel = null;
        this._scrumTeamViewModel = null;
        this._memberInfo = {
            teamName: ko.observable(null),
            memberName: ko.observable(null)
        };
        this._lastMessageId = 0;
        this._getMessagesErrorCount = 0;
    };

    planningPoker.prototype = {
        _createTeamViewModel: null,
        _joinTeamViewModel: null,
        _scrumTeamViewModel: null,
        _memberInfo: null,
        _lastMessageId: 0,
        initialize: function () {
            var t = this._t;

            t._createTeamViewModel = null;
            t._joinTeamViewModel = null;
            t._scrumTeamViewModel = null;
            t._memberInfo = {
                teamName: ko.observable(null),
                memberName: ko.observable(null)
            };
            t._lastMessageId = 0;

            $(document)
                .ajaxStart(function () { $.blockUI({ message: "Processing..." }) })
                .ajaxStop($.unblockUI)
                .ajaxError(t._processAjaxError);

            $.ajax({
                context: t,
                url: t._templateUrl,
                dataType: "html",
                success: t._initializeTemplate
            });
        },
        _processAjaxError: function (event, jqXHR, ajaxSettings, thrownError) {
            var errorXml = null;
            if (typeof (jqXHR.responseXML) == "undefined" || jqXHR.responseXML == null) {
                errorXml = $.parseXML(jqXHR.responseText);
            }
            else {
                errorXml = jqXHR.responseXML;
            }
            var errorMessage = $(errorXml).children("Fault").children("Reason").text();
            if (errorMessage != "") {
                errorMessage = errorMessage.split("\n", 1)[0];
                var globalMessagePanel = $("#globalMessagePanel");
                $("#globalMessageTitle", globalMessagePanel).text("Error");
                $("#globalMessageContainer", globalMessagePanel).text(errorMessage);
                globalMessagePanel.modal("show");
            }
        },
        _initializeTemplate: function (data) {
            var t = this._t;
            $(t._container).html(data);
            t._initializeCreateTeam();
            t._initializeJoinTeam();
        },
        _initializeCreateTeam: function () {
            var t = this._t;
            t._createTeamViewModel = {
                teamName: ko.observable(""),
                scrumMasterName: ko.observable("")
            };
            $("#userInfoPanel", t._container).hide();
            $("#membersPanel", t._container).slideUp();
            $("#pokerDeskPanel", t._container).slideUp();
            var createTeamPanel = $("#createTeamPanel", t._container);
            createTeamPanel.slideDown();
            ko.applyBindings(t._createTeamViewModel, createTeamPanel.get(0));
            $("form", createTeamPanel)
                .submit(t, t._createTeamSubmit)
                .validate({
                    errorClass: "has-error has-feedback",
                    validClass: "",
                    errorContainer: "#createTeamPanel div.alert-danger",
                    highlight: function (element, errorClass, validClass) {
                        $(element).closest(".form-group").removeClass(validClass).addClass(errorClass);
                    },
                    unhighlight: function (element, errorClass, validClass) {
                        $(element).closest(".form-group").removeClass(errorClass).addClass(validClass);
                    },
                    showErrors: function (errorMap, errorList) {
                        $("#createTeamPanel div.alert-danger span").text("Enter required values and try again.");
                        this.defaultShowErrors();
                    }
                });
        },
        _initializeJoinTeam: function () {
            var t = this._t;
            t._joinTeamViewModel = {
                teamName: ko.observable(""),
                memberName: ko.observable(""),
                asObserver: ko.observable(false)
            };
            $("#userInfoPanel", t._container).hide();
            $("#membersPanel", t._container).slideUp();
            $("#pokerDeskPanel", t._container).slideUp();
            var joinTeamPanel = $("#joinTeamPanel", t._container);
            joinTeamPanel.slideDown();
            ko.applyBindings(t._joinTeamViewModel, joinTeamPanel.get(0));
            $("form", joinTeamPanel)
                .submit(t, t._joinTeamSubmit)
                .validate({
                    errorClass: "has-error has-feedback",
                    validClass: "",
                    errorContainer: "#joinTeamPanel div.alert-danger",
                    highlight: function (element, errorClass, validClass) {
                        $(element).closest(".form-group").removeClass(validClass).addClass(errorClass);
                    },
                    unhighlight: function (element, errorClass, validClass) {
                        $(element).closest(".form-group").removeClass(errorClass).addClass(validClass);
                    },
                    showErrors: function (errorMap, errorList) {
                        $("#joinTeamPanel div.alert-danger span").text("Enter required values and try again.");
                        this.defaultShowErrors();
                    }
                });
        },
        _initializePokerDesk: function (st) {
            var t = this._t;
            $("#createTeamPanel, #joinTeamPanel", t._container).slideUp();

            var userInfoPanel = $("#userInfoPanel", t._container);
            $("a[href='#disconnect']", userInfoPanel).click(t, t._disconnectClick);
            userInfoPanel.show();

            var membersPanel = $("#membersPanel", t._container);
            membersPanel.slideDown();

            var pokerDeskPanel = $("#pokerDeskPanel", t._container);
            $("a[href='#startEstimation']", pokerDeskPanel).click(t, t._startEstimationClick);
            $("a[href='#cancelEstimation']", pokerDeskPanel).click(t, t._cancelEstimationClick);
            pokerDeskPanel.slideDown();

            ko.applyBindings(st, pokerDeskPanel.get(0));
            ko.applyBindings(st, userInfoPanel.get(0));
            ko.applyBindings(st, membersPanel.get(0));

            t._getMessagesErrorCount = 0;
            t._getMessages(null);
        },
        _createTeamSubmit: function (e) {
            var t = e.data;
            e.preventDefault();

            if ($(this).valid()) {
                t._memberInfo.teamName(t._createTeamViewModel.teamName());
                t._memberInfo.memberName(t._createTeamViewModel.scrumMasterName());

                var createTeamData = {
                    teamName: t._createTeamViewModel.teamName(),
                    scrumMasterName: t._createTeamViewModel.scrumMasterName()
                };
                var url = t._serviceUrl + "/CreateTeam?" + $.param(createTeamData);
                $.ajax({
                    context: t,
                    url: url,
                    dataType: "json",
                    success: t._createTeamSuccess
                });
            }
        },
        _createTeamSuccess: function (data) {
            var t = this._t;
            t._lastMessageId = 0;
            var st = t._createScrumTeamViewModel(data);
            t._initializePokerDesk(st);
        },
        _joinTeamSubmit: function (e) {
            var t = e.data;
            e.preventDefault();

            if ($(this).valid()) {
                t._memberInfo.teamName(t._joinTeamViewModel.teamName());
                t._memberInfo.memberName(t._joinTeamViewModel.memberName());

                var joinTeamData = {
                    teamName: t._joinTeamViewModel.teamName(),
                    memberName: t._joinTeamViewModel.memberName(),
                    asObserver: t._joinTeamViewModel.asObserver()
                };
                var url = t._serviceUrl + "/JoinTeam?" + $.param(joinTeamData);
                $.ajax({
                    context: t,
                    url: url,
                    dataType: "json",
                    success: t._joinTeamSuccess
                });
            }
        },
        _joinTeamSuccess: function (data) {
            var t = this._t;
            t._lastMessageId = 0;
            var st = t._createScrumTeamViewModel(data);
            t._initializePokerDesk(st);
        },
        _disconnectClick: function (e) {
            var t = e.data;
            e.preventDefault();

            var disconnectTeamData = {
                teamName: t._memberInfo.teamName(),
                memberName: t._memberInfo.memberName()
            };
            var url = t._serviceUrl + "/DisconnectTeam?" + $.param(disconnectTeamData);
            $.ajax({
                context: t,
                url: url,
                dataType: "text",
                success: t._disconnectTeamSuccess
            });
        },
        _disconnectTeamSuccess: function (data) {
            var t = this._t;
            t.initialize();
        },
        _startEstimationClick: function (e) {
            var t = e.data;
            if (t._scrumTeamViewModel.canStartEstimation()) {
                var startEstimationData = {
                    teamName: t._memberInfo.teamName()
                }
                var url = t._serviceUrl + "/StartEstimation?" + $.param(startEstimationData);
                $.ajax({
                    context: t,
                    url: url,
                    dataType: "text",
                    success: t._startEstimationSuccess
                });
            }
        },
        _startEstimationSuccess: function (data) {
            // do nothing. just wait for message
        },
        _cancelEstimationClick: function (e) {
            var t = e.data;
            if (t._scrumTeamViewModel.canCancelEstimation()) {
                var cancelEstimationData = {
                    teamName: t._memberInfo.teamName()
                }
                var url = t._serviceUrl + "/CancelEstimation?" + $.param(cancelEstimationData);
                $.ajax({
                    context: t,
                    url: url,
                    dataType: "text",
                    success: t._cancelEstimationSuccess
                });
            }
        },
        _cancelEstimationSuccess: function (data) {
            // do nothing. just wait for message
        },
        selectEstimation: function (estimation) {
            var t = this._t;
            t._scrumTeamViewModel.selectedEstimation(estimation);

            var submitEstimationData = {
                teamName: t._memberInfo.teamName(),
                memberName: t._memberInfo.memberName(),
                estimation: estimation.value() != null ? estimation.value() : -1111111
            };
            var url = t._serviceUrl + "/SubmitEstimation?" + $.param(submitEstimationData);
            $.ajax({
                context: t,
                url: url,
                dataType: "text",
                success: t._selectEstimationSuccess
            });
        },
        _selectEstimationSuccess: function (data) {
            // do nothing. just wait for message
        },
        kickoffMember: function (member, team) {
            var t = this._t;
            var disconnectTeamData = {
                teamName: team.name(),
                memberName: member.name()
            };
            var url = t._serviceUrl + "/DisconnectTeam?" + $.param(disconnectTeamData);
            $.ajax({
                context: t,
                url: url,
                dataType: "text",
                success: t._kickoffMemberSuccess
            });
        },
        _kickoffMemberSuccess: function (data) {
            // do nothing. just wait for message
        },
        _getMessages: function (data) {
            var t = this._t;

            if (t._memberInfo != null && t._memberInfo.teamName() != null && t._memberInfo.memberName() != null) {
                if (data != null) {
                    t._getMessagesErrorCount = 0;
                    t._processMessages(data);
                }

                var getMessagesData = {
                    teamName: t._memberInfo.teamName(),
                    memberName: t._memberInfo.memberName(),
                    lastMessageId: t._lastMessageId
                };
                var url = t._serviceUrl + "/GetMessages?" + $.param(getMessagesData);
                $.ajax({
                    global: false,
                    context: t,
                    url: url,
                    dataType: "json",
                    success: t._getMessages,
                    error: t._onErrorGetMessages
                });
            }
        },
        _onErrorGetMessages: function () {
            var t = this._t;
            if (t._getMessagesErrorCount <= 5) {
                t._getMessagesErrorCount++;
                window.setTimeout(function () { t._getMessages(null); }, 5000);
            }
        },
        _processMessages: function (data) {
            var t = this._t;
            for (var i = 0; i < data.length; i++) {
                var message = data[i];
                if (t._lastMessageId < message.id) {
                    t._lastMessageId = message.id;
                }

                switch (message.type) {
                    case 1: // MemberJoined
                        t._scrumTeamViewModel.joinMember(message.member.name, message.member.type == "Observer");
                        break;
                    case 2: // MemberDisconnected
                        t._scrumTeamViewModel.disconnectMember(message.member.name);
                        break;
                    case 3: // EstimationStarted
                        t._scrumTeamViewModel.startEstimation();
                        break;
                    case 4: // EstimationEnded
                        var estimationResultItems = [];
                        for (var i = 0; i < message.estimationResult.length; i++) {
                            var estimationResultItem = message.estimationResult[i];
                            var eri = new EstimationResultItem(estimationResultItem.member.name);
                            if (estimationResultItem.estimation != null) {
                                eri.estimation(new Estimation(estimationResultItem.estimation.value));
                            }
                            estimationResultItems.push(eri);
                        }
                        t._scrumTeamViewModel.endEstimation(estimationResultItems);
                        break;
                    case 5: // EstimationCanceled
                        t._scrumTeamViewModel.cancelEstimation();
                        break;
                    case 6: // MemberEstimated
                        t._scrumTeamViewModel.memberEstimated(message.member.name);
                        break;
                }
            }
        },
        _createScrumTeamViewModel: function (data) {
            var t = this._t;

            var st = new ScrumTeam(data.name, t._memberInfo.memberName());
            if (data.scrumMaster != null) {
                st.scrumMaster(new TeamMember(data.scrumMaster.name));
            }
            if (data.members != null) {
                for (var i = 0; i < data.members.length; i++) {
                    var member = data.members[i];
                    if (member.type != "ScrumMaster") {
                        st.members.push(new TeamMember(member.name));
                    }
                }
            }
            if (data.observers != null) {
                for (var i = 0; i < data.observers.length; i++) {
                    var observer = data.observers[i];
                    st.observers.push(new TeamMember(observer.name));
                }
            }

            st.state(data.state);

            if (data.avilableEstimations != null) {
                for (var i = 0; i < data.avilableEstimations.length; i++) {
                    var estimation = new Estimation(data.avilableEstimations[i].value);
                    st.availableEstimations.push(estimation);
                }
            }

            t._scrumTeamViewModel = st;
            st.serviceProvider = t;
            return st;
        }
    }

    window.Duracellko = {
        PlanningPoker: planningPoker
    };

})(window);
