var Duracellko;
(function (Duracellko) {
    // Static class providing functionality for Planning Poker application.
    var PlanningPoker = (function () {
        function PlanningPoker() {
        }

        PlanningPoker.credentialsCookieName = "PlanningPoker.Credentials";
        PlanningPoker.cookieExpiration = 900000;

        // Shows message box using jQuery.
        PlanningPoker.showMessageBox = function (element) {
            $(element).modal({ backdrop: 'static' });
        };

        // Shows busy indicator using jQuery.
        PlanningPoker.showBusyIndicator = function (element) {
            $(element).modal({ backdrop: 'static', keyboard: false });
        };

        // Hides element using jQuery.
        PlanningPoker.hide = function (element) {
            $(element).modal('hide');
        };

        // Loads member credentials of connected user from cookie.
        PlanningPoker.getMemberCredentials = function () {
            let cookie = document.cookie;
            let cookiePairs = cookie.split(';');
            for (let i = 0; i < cookiePairs.length; i++) {
                let cookiePair = cookiePairs[i].trim();
                let startText = PlanningPoker.credentialsCookieName + '=';
                if (cookiePair.startsWith(startText)) {
                    let value = cookiePair.substr(startText.length);
                    return decodeURIComponent(value);
                }
            }

            return null;
        };

        // Saves member credentials of connected user into cookie.
        PlanningPoker.setMemberCredentials = function (credentials) {
            if (credentials === null) {
                credentials = '';
            }
            let cookie = PlanningPoker.credentialsCookieName + '=' + encodeURIComponent(credentials);

            let expiration = new Date(Date.now() + PlanningPoker.cookieExpiration);
            cookie += "; expires=" + expiration.toUTCString();
            cookie += "; path=/";

            document.cookie = cookie;
        };

        return PlanningPoker;
    })();

    Duracellko.PlanningPoker = PlanningPoker;
})(Duracellko || (Duracellko = {}));
