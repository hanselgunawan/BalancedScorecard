<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="view_request_kpi.aspx.cs" Inherits="Balanced_Scorecard.view_request_kpi" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    View Request for Change Individual KPI
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Respond Requests</li>
                              <li><a runat="server" id="breadcrumb_kpi_approval">KPIs</a></li>
                              <li><b>View KPI Request Details</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-list"></i>&nbsp;View KPI Request Details</h1>
                        </div>
                    </div>
                <!-- /.row -->
                <a id="hrefBackToKPIApproval" class="btn btn-primary" runat="server" style="margin-top:10px"><i class="fa fa fa-reply"></i>&nbsp;Back to Approve KPI Add/Change/Delete Request</a>
                    <div class="row">
                        <div class="col-md-6 col-xs-6">
					        <div class="panel panel-primary" id="panel_csc">
                                <div class="panel-heading"><p class="csc_p">User & BSC Info</p></div>
                                    <div class="table-responsive" id="UserTable" runat="server">
                                        <table class="table table-bordered">
                                            <asp:PlaceHolder ID="PlaceHolderUser" runat="server"></asp:PlaceHolder>
                                        </table>
                                    </div>
                            </div>
                        </div>
                    </div>
                    <p style="margin-bottom:10px"><b>Reason:</b> <asp:Label ID="LabelReason" runat="server" Text="Label"></asp:Label></p>
                    <asp:PlaceHolder ID="PlaceHolderBefore" runat="server"></asp:PlaceHolder>
                    <asp:PlaceHolder ID="PlaceHolderAfter" runat="server"></asp:PlaceHolder>
                    <form runat="server">
                    <div class="form-group">
                      	<div class="col-md-10">
                               <span style="color:white; margin-left:20px; width:150px; background-color:#ff5050" class="btn btn-add-group btn-add-group-container add-button" id="SpanRejectRequest" runat="server">
                                <i class="fa fa-fw fa-pencil-square-o"></i>
                                  <asp:Button BackColor="Transparent" style="background-color:Transparent;border-color:Transparent; width:150px" BorderColor="Transparent" ID="ButtonReject" CssClass="relative-to-btn" OnClick="OnClickRejectRequest" Text="Reject Request" runat="server" />
                              </span>
                              <span style="color:white; margin-left:100px; width:170px; background-color:green" class="btn btn-add-group btn-add-group-container add-button" id="SpanApproveRequest" runat="server">
                                <i class="fa fa-fw fa-pencil-square-o"></i>
                                  <asp:Button BackColor="Transparent" style="background-color:Transparent;border-color:Transparent; width:165px" BorderColor="Transparent" ID="ButtonApprove" CssClass="relative-to-btn" OnClick="OnClickApproveRequest" Text="Approve Request" runat="server" />
                              </span>
                        </div>
                      </div>
                    </form>
            <!-- /.container-fluid -->
        </div>
        <!-- /#page-wrapper -->
</asp:Content>
