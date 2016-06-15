<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="view_so_history_detail.aspx.cs" Inherits="Balanced_Scorecard.view_so_history_detail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    View Specific Objective Change Requests History | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Requests History</li>
                              <li><a runat="server" id="breadcrumb_change_request_so">Specific Objective Requests History</a></li>
                              <li><b>View Details of Specific Objective Request History</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-list"></i>&nbsp;View Details of Specific Objective Request History</h1>
                        </div>
                    </div>
                <!-- /.row -->
                <a id="hrefBackToSOChangeRequest" class="btn btn-primary" runat="server" style="margin-top:10px; margin-bottom:30px"><i class="fa fa fa-reply"></i>&nbsp;Back to Specific Objective Requests History</a>
				<p><b>Reason:</b> <asp:Label ID="LabelReason" runat="server" Text="Label"></asp:Label></p>	
                <asp:PlaceHolder ID="PlaceHolderBefore" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="PlaceHolderAfter" runat="server"></asp:PlaceHolder>
            <!-- /.container-fluid -->
        </div>
        <!-- /#page-wrapper -->
</asp:Content>
