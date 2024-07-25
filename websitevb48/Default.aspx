<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login Page</title>
    <script type="text/javascript"
        src="https://go.playerzero.app/record/666af2fef6b93a24518cf726"
        async crossorigin></script>
    
    <script type="text/javascript">
        const userId = "new-website-vb-user";
        const metadata = {
            name: "new-website-vb-name",
            email: "new-website-vb@email.user"
        };
        const setCookie = (t) => document.cookie = `pz-traceid=${t}; Path=/;`;

        if (window.playerzero) {
            // PlayerZero has loaded
            window.playerzero.identify(userId, metadata);
            window.playerzero.onTraceChange(setCookie);
        } else {
            // PlayerZero has not loaded, so we'll wait for the ready event
            window.addEventListener(
                "playerzero_ready",
                () => {
                    window.playerzero.identify(userId, metadata);
                    window.playerzero.onTraceChange(setCookie);
                },
                { once: true }
            );
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Login</h2>
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
            <div>
                <asp:Label ID="lblUsername" runat="server" Text="Username:"></asp:Label>
                <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
            </div>
            <div>
                <asp:Label ID="lblPassword" runat="server" Text="Password:"></asp:Label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox>
            </div>
            <div>
                <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLogin_Click" />
            </div>
        </div>
    </form>
</body>
</html>
