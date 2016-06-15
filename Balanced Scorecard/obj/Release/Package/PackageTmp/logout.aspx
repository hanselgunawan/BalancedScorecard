<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="logout.aspx.cs" Inherits="Balanced_Scorecard.logout" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="canonical" href="index.aspx"/>
    <meta charset="utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge"/>
    <meta name="viewport" content="width=device-width, initial-scale=1"/>
    <meta name="description" content=""/>
    <meta name="author" content=""/>
    <meta http-equiv="Cache-Control" content="no-cache"/>
    <meta http-equiv="Pragma" content="no-cache"/>
    <meta http-equiv="Expires" content="0"/>
    <meta http-equiv="refresh" content="0;url=index.aspx"/>
    <title>Log Out | Balanced Scorecard</title>
    <!--<script type="text/javascript">
        //function Redirect() {
            //window.location.href("index.aspx"); <---BISA DI IE
            //location.assign("index.aspx"); // <-- BISA DI Chrome & Firefox
            //document.location = "index.aspx";
        //}
        /*window.onload = function () {
            window.location.href = 'index.aspx';
        };
        window.onunload = function () { };
        window.location.href = 'index.aspx';*/
        window.history.forward(1);
        function noBack() {
            window.history.forward();
        }
    </script>-->
    <script type="text/javascript">
        var url = "index.aspx";
        // IE8 and lower fix
        if (navigator.userAgent.match(/MSIE\s(?!9.0)/)) {
            var referLink = document.createElement("a");
            referLink.href = url;
            document.body.appendChild(referLink);
            referLink.click();
        }
            // All other browsers
        else { window.location.replace(url); }
    </script>
<!-- Redirect Ends -->
</head>
<body>
    <form id="form1" runat="server">
        <div>
        </div>
    </form>
</body>
</html>
