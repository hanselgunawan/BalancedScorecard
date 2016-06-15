<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="view_my_kpi_request.aspx.cs" Inherits="Balanced_Scorecard.view_my_kpi_request" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    View My KPI Requests | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>My Requests</li>
                              <li><a runat="server" id="breadcrumb_change_request_kpi">KPIs</a></li>
                              <li><b>View Details of Requested KPI</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-list"></i>&nbsp;View Details of Requested KPI</h1>
                        </div>
                    </div>
                <!-- /.row -->
                <a id="hrefBackToKPIChangeRequest" class="btn btn-primary" runat="server" style="margin-top:10px; margin-bottom:30px"><i class="fa fa fa-reply"></i>&nbsp;Back to Add/Change/Delete Requests for KPI</a>
                <p style="margin-top:-10px; margin-bottom:10px"><b>Reason:</b> <asp:Label ID="LabelReason" runat="server" Text="Label"></asp:Label></p>
                <asp:PlaceHolder ID="PlaceHolderBefore" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="PlaceHolderAfter" runat="server"></asp:PlaceHolder>
            <!-- /.container-fluid -->
        </div>
        <!-- /#page-wrapper -->
</asp:Content>
