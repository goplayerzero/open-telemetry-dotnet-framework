<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/Site1.Master" CodeBehind="About.aspx.vb" Inherits="WebsiteVBNET481.About" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>About Us</h2>
    <p>Welcome to the About page of our website. Here is the user data fetched from the API:</p>
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="True" />

    <p>

        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>

    </p>
    <p>

        <asp:Button ID="Button1" runat="server" Text="Button" />

    </p>
</asp:Content>