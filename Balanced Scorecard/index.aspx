<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="Balanced_Scorecard.index" %>

<!DOCTYPE html> 

<html xmlns="http://www.w3.org/1999/xhtml">
<html lang="en">
<head runat="server">
    <meta charset="utf-8"/>	
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content=""/>
    <meta name="author" content=""/>
    <meta http-equiv="Cache-Control" content="no-cache"/>
    <meta http-equiv="Pragma" content="no-cache"/>
    <meta http-equiv="Expires" content="0"/>

    <title>Welcome to MPPA Balanced Scorecard</title>
    <link rel="stylesheet" type="text/css" href="css/bootstrap.css" />
    <link rel="stylesheet" type="text/css" href="css/style.css" />

	<!-- jQuery -->
    <script src="js/jquery.js"></script>
    <script src="js/bootstrap.js"></script>

    <script type="text/javascript">
        function DisableBackButton() {
            window.history.forward()
        }
        DisableBackButton();
        window.onload = DisableBackButton;
        window.onpageshow = function (evt) { if (evt.persisted) DisableBackButton() }
        window.onunload = function () { void (0) }
    </script>
</head>
<body id="body-bg-login">
    <div class="container-fluid">
    	<div id="login-box" class="container">
        	<img src="Images/mppa.png" class="img-responsive" id="img-logo-login"/>
            <form runat="server" id="LoginForm">
                <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            	<div class="form-group">
                    <asp:TextBox ID="TextBoxNIK" CssClass="form-control" required="required" placeholder="NIK" runat="server"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="TextBoxPassword" required="required" CssClass="form-control" placeholder="Password" TextMode="Password" runat="server"></asp:TextBox>
                </div>
                <asp:UpdatePanel ID="UpdatePanelErrorMessage" runat="server">
                <ContentTemplate>
                    <p runat="server" id="ErrorLoginMessage"></p>
                    <asp:Button ID="ButtonLogin" runat="server" OnClick="OnClickLogin" CssClass="btn btn-logo" Text="LOG IN" />
                </ContentTemplate>
                </asp:UpdatePanel>
                <p style="color:white"><b>Forgot Password?</b> Please Call The Administrator At (021) 5459922</p>
            </form>
            
        </div>
    </div>
</body>
</html>

<!-- Developed by Hansel Bagus Tritama from Indonesia -->
<!-- Contact me at hanselgunawan94@gmail.com for business -->
<!-- ENJOY THE APP! :) -->