<%@ Page Language="VB" AutoEventWireup="false" CodeFile="About.aspx.vb" Inherits="About" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
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
        <h2>About Us</h2>
        <p>Welcome to the About page of our website. Here is the user data fetched from the API:</p>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="True" />

        <p>

            <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>

        </p>
        <p>

            <asp:Button ID="Button1" runat="server" Text="Button" />

        </p>
    </div>
    </form>
</body>
</html>
