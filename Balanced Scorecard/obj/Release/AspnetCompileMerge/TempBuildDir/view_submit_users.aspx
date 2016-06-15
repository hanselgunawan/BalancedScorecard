<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="view_submit_users.aspx.cs" Inherits="Balanced_Scorecard.view_submit_users" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    View Submit Users | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><a id="hrefDashboard" runat="server">Scorecard Dashboard</a></li>
                              <li><b>View Submit Users</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-user"></i>&nbsp;View Submit Users</h1>
                        </div>
                    </div>
                <!-- /.row -->
                    <a id="hrefBackToDashboard" class="btn btn-primary" runat="server" style="margin-top:0px; margin-bottom:-140px"><i class="fa fa fa-reply"></i>&nbsp;Back to Dashboard</a>
					<form runat="server">
                    <div style="text-align:right">
                        <asp:Button CssClass="btn btn-success" ID="ButtonExcel" runat="server" Text="Export All BSC Scores to Excel" OnClick="OnClickExportExcel"/>
                    </div>
                    <div class="row" style="margin-top:10px">
                      <div class="col-xs-4 col-md-4">
                      </div>
                      <div class="col-xs-3 col-md-3">
                          <asp:DropDownList ID="DropDownListSearchBy" CssClass="form-control" runat="server"></asp:DropDownList>
                      </div>
                      <div class="col-xs-5 col-md-5">
                          <div class="input-group">
                              <div class="input-group-addon" style="background-color:transparent"><i class="fa fa fa-search"></i></div>
                              <input type="text" id="TextBoxSearch" runat="server" class="form-control search-query" /> 
                              <span class="input-group-btn">
                                  <asp:Button ID="ButtonSearch" OnClick="OnClickSearch" runat="server" CssClass="btn btn-search" Text="Search" />
                              </span>
                          </div>
                      </div>
                    </div>
                    <div class="panel panel-primary" id="panel_csc">
                        	<div class="panel-heading" style="height:40px">
                                <table>
                                    <tr>
                                        <td>
                                        <asp:Label ID="Label1" Font-Bold="true" CssClass="control-label" runat="server" Text="List of Scorecard Users - Period: "></asp:Label>
                                        </td>
                                        <td>&nbsp;&nbsp;&nbsp;&nbsp;</td>
                                        <td>
                                        <div class="btn-group" role="group">
                                            <asp:Label ID="LabelPeriod" runat="server" Text="Label"></asp:Label>
                                        </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <div class="table-responsive">
                              <table class="table table-bordered">
                                 <asp:PlaceHolder ID="PlaceHolderTable" runat="server"></asp:PlaceHolder>
                              </table>
                            </div>
                    </div>
                    </form>
                    <div class="my-paging">
                        <asp:PlaceHolder ID="PlaceHolderPaging" runat="server"></asp:PlaceHolder>       
                   </div>
            </div>
            <!-- /.container-fluid -->

        </div>
        <!-- /#page-wrapper -->
</asp:Content>
