<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="dashboard.aspx.cs" Inherits="Balanced_Scorecard.dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Scorecard Dashboard | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
        <div class="container-fluid">
            <!-- Breadcrumb -->
            <div class="row">
                <div class="col-lg-12">
                    <ol class="breadcrumb" id="breadcrumb-csc">
                        <li><b>Scorecard Dashboard</b></li>
    				</ol>
                    <h1><i class="fa fa fa-home"></i>&nbsp;Scorecard Dashboard</h1><br/>
                </div>
            </div>
            <!-- Page Heading -->

            <form class="form-horizontal" id="PeriodForm" runat="server">
                <asp:ScriptManager ID="ScriptManagerDashboard" runat="server"></asp:ScriptManager>
                <asp:UpdatePanel ID="UpdatePanelSetPeriod" runat="server">
                <ContentTemplate>
                <div class="form-group">
                <label for="start_date" class="col-sm-2 col-md-1 control-label">Period</label>
                    <div class="col-sm-5 col-md-3">
                         <asp:PlaceHolder ID="PlaceHolderPeriod" runat="server"></asp:PlaceHolder>
                    </div>
                </div>
                </ContentTemplate>
                </asp:UpdatePanel>
            </form>

            <div style="font-weight:bold;font-size:large" runat="server" id="labelsummary">Scorecard Users</div>
              <div class="row" id="summary" runat="server">
                    <div class="col-lg-6 col-md-6">
                        <div class="panel panel-primary">
                            <div class="panel-heading">
                                <div class="row">
                                    <div class="col-xs-3">
                                        <i class="fa fa-user fa-5x"></i>
                                    </div>
                                    <div class="col-xs-9 text-right">
                                        <div class="huge">
                                            <asp:Label ID="LabelHaveSubmit" runat="server"></asp:Label>
                                        </div>
                                        <div>
                                           out of  <asp:Label ID="LabelTotalHaveSubmit" runat="server" Font-Size="Medium" Font-Bold="true"></asp:Label> employee
                                        </div>
                                        <div>
                                            <b>have submitted</b> BSC
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <a id="hrefSubmit" runat="server">
                                <div class="panel-footer">
                                    <span class="pull-left">View Details</span>
                                    <span class="pull-right"><i class="fa fa-arrow-circle-right"></i></span>
                                    <div class="clearfix"></div>
                                </div>
                            </a>
                        </div>
                    </div>

                   <div class="col-lg-6 col-md-6" >
                        <div class="panel panel-red">
                            <div class="panel-heading">
                                <div class="row">
                                    <div class="col-xs-3">
                                        <i class="fa fa-user fa-5x"></i>
                                    </div>
                                    <div class="col-xs-9 text-right">
                                        <div class="huge">
                                             <asp:Label ID="LabelNotSubmit" runat="server"></asp:Label>
                                        </div>
                                        <div>
                                             out of  <asp:Label ID="LabelTotalNotSubmit" runat="server" Font-Size="Medium" Font-Bold="true"></asp:Label> employee
                                        </div>
                                        <div>
                                            <b>have not submitted</b> BSC
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <a id="hrefNotSubmit" runat="server" >
                                <div class="panel-footer">
                                    <span class="pull-left">View Details</span>
                                    <span class="pull-right"><i class="fa fa-arrow-circle-right"></i></span>
                                    <div class="clearfix"></div>
                                </div>
                            </a>
                        </div>
                    </div>
                  </div> <!--row-->
        </div>
        <!-- /.container-fluid -->
    </div>
    <!-- /#page-wrapper -->
</asp:Content>
