<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DateTest.aspx.cs" Inherits="Balanced_Scorecard.DateTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <input type="date" id="StartDate" runat="server"/>
        <input type="date" id="EndDate" runat="server"/>
        <asp:Button ID="ButtonSave" OnClick="OnClickSave" runat="server" Text="Save" />
        <asp:Label ID="LabelDate" runat="server" Text="Label"></asp:Label>
    </div>
    </form>
</body>
</html>
