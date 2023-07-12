var Duracellko;
(function (Duracellko) {
    // Static class providing functionality for Planning Poker application.
    var PlanningPoker = (function () {
        function PlanningPoker() {
        }

        PlanningPoker.credentialsTeamNameKey = "PlanningPoker.Credentials.TeamName";
        PlanningPoker.credentialsMemberNameKey = "PlanningPoker.Credentials.MemberName";
        PlanningPoker.timerDurationKey = "PlanningPoker.TimerSettings.TimerDuration";

        // Shows message box using Bootstrap.
        PlanningPoker.showMessageBox = function (element) {
            const options = { backdrop: 'static' };
            const modal = bootstrap.Modal.getOrCreateInstance(element, options);
            modal.show();
        };

        // Shows busy indicator using Bootstrap.
        PlanningPoker.showBusyIndicator = function (element) {
            const options = { backdrop: 'static', keyboard: false };
            const modal = bootstrap.Modal.getOrCreateInstance(element, options);
            modal.show();
        };

        // Hides modal component using Bootstrap.
        PlanningPoker.hide = function (element) {
            const modal = bootstrap.Modal.getOrCreateInstance(element);
            modal.hide();
        };

        // Registers event handler object for Bootstrap Modal Hidden event.
        PlanningPoker.registerOnModalHidden = function (element, handler) {
            element.addEventListener('hidden.bs.modal', function () {
                handler.invokeMethodAsync('OnModalHidden');
            });
        }

        // Loads member credentials of connected user from Session or LocalStorage.
        PlanningPoker.getMemberCredentials = function (permanentScope) {
            let teamName = window.sessionStorage.getItem(PlanningPoker.credentialsTeamNameKey);
            if (permanentScope && !teamName) {
                teamName = window.localStorage.getItem(PlanningPoker.credentialsTeamNameKey);
            }

            let memberName = window.sessionStorage.getItem(PlanningPoker.credentialsMemberNameKey);
            if (permanentScope && !memberName) {
                memberName = window.localStorage.getItem(PlanningPoker.credentialsMemberNameKey);
            }

            if (teamName && memberName) {
                return {
                    teamName,
                    memberName
                };
            }

            return null;
        };

        // Saves member credentials of connected user into Session and LocalStorage.
        PlanningPoker.setMemberCredentials = function (credentials) {
            if (credentials) {
                window.sessionStorage.setItem(PlanningPoker.credentialsTeamNameKey, credentials.teamName);
                window.sessionStorage.setItem(PlanningPoker.credentialsMemberNameKey, credentials.memberName);
                window.localStorage.setItem(PlanningPoker.credentialsTeamNameKey, credentials.teamName);
                window.localStorage.setItem(PlanningPoker.credentialsMemberNameKey, credentials.memberName);
            }
            else {
                // When user disconnects, only Session is removed. LocalStorage credentials are persisted for next session.
                window.sessionStorage.removeItem(PlanningPoker.credentialsTeamNameKey);
                window.sessionStorage.removeItem(PlanningPoker.credentialsMemberNameKey);
            }
        };

        // Loads timer duration setting from LocalStorage.
        PlanningPoker.getTimerDuration = function () {
            return window.localStorage.getItem(PlanningPoker.timerDurationKey);
        };

        // Saves timer duration setting into LocalStorage.
        PlanningPoker.setTimerDuration = function (timerDuration) {
            window.localStorage.setItem(PlanningPoker.timerDurationKey, timerDuration);
        };

        return PlanningPoker;
    })();

    Duracellko.PlanningPoker = PlanningPoker;
})(Duracellko || (Duracellko = {}));
