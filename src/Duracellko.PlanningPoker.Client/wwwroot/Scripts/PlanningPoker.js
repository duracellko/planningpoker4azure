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
            try {
                const options = { backdrop: 'static', keyboard: false };
                const modal = bootstrap.Modal.getOrCreateInstance(element, options);
                modal.show();
            }
            catch (ex) {
                // Bootstrap may not be initialized, when busy indicator is requested.
                // It can be ignored, that it is not shown the first time.
                console.error("Showing BusyIndicator failed.")
                console.error(ex);
            }
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

        // Posts estimation result to the calling application.
        PlanningPoker.postEstimationResult = function(estimation, callbackReference) {
            const message = {
                estimation: estimation,
                reference: callbackReference.reference
            }
            window.opener.postMessage(message, callbackReference.url);
            window.opener.focus();
        }

        return PlanningPoker;
    })();

    var PlanningPokerLoader = (function () {
        function PlanningPokerLoader() {
        }

        PlanningPokerLoader.blazorResourceKey = "blazor-resource-hash:Duracellko.PlanningPoker.Client";
        PlanningPokerLoader.appElementId = "app";
        PlanningPokerLoader.loadingElementId = "duracellko-planningpoker-app-loader";
        PlanningPokerLoader.defaultLoadingTimeout = 3000; // 3000 ms

        // Starts timing of client-side loading of Blazor application.
        // If the loading takes too long then the application is redirected to server-side version.
        PlanningPokerLoader.startClientSideLoadingWatchdog = function (timeout) {
            const blazorResourceHash = window.localStorage.getItem(PlanningPokerLoader.blazorResourceKey);
            if (!blazorResourceHash) {
                // Blazor is using server-side rendering, because the resource hash is not stored in LocalStorage.
                // No need to start client-side loading watchdog.
                return;
            }

            if (!timeout)
            {
                timeout = PlanningPokerLoader.defaultLoadingTimeout;
            }

            window.setTimeout(PlanningPokerLoader.onLoadingTimeout, timeout);
        }

        PlanningPokerLoader.onLoadingTimeout = function () {
            const appElement = window.document.getElementById(PlanningPokerLoader.appElementId);
            const loadingElement = window.document.getElementById(PlanningPokerLoader.loadingElementId);
            if (!!appElement && !!loadingElement && loadingElement.parentElement === appElement) {
                // Application is still loading, so it is redirected to server-side version.
                window.localStorage.removeItem(PlanningPokerLoader.blazorResourceKey);
                window.location.reload();
            }
        }

        return PlanningPokerLoader;
    })();

    Duracellko.PlanningPoker = PlanningPoker;
    Duracellko.PlanningPokerLoader = PlanningPokerLoader;
})(Duracellko || (Duracellko = {}));
