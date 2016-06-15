<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="scorecard_user.aspx.cs" Inherits="Balanced_Scorecard.scorecard_user" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Scorecard User | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>User Management</li>
                              <li><b>Scorecard Users</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-user"></i>&nbsp;Scorecard Users</h1>
                        </div>
                    </div>
                <!-- /.row -->
                    <a id="btnImportExcel" runat="server" style="margin-top:-70px; margin-bottom:-120px"><i class="fa fa fa-file-excel-o"></i>&nbsp;&nbsp;Import from Excel</a>
                    <a id="btnAddMeasure" runat="server" style="margin-top:20px; margin-bottom:-120px; margin-left:-160px"><i class="fa fa fa-plus"></i>&nbsp;&nbsp;Add User</a>
					<form id="FormExport" runat="server">
                        <div style="text-align:right">
                            <asp:Button CssClass="btn btn-danger" ID="ButtonPDF" runat="server" Text="Export to PDF" OnClick="OnClickExportPDF"/>
                            <asp:Button CssClass="btn btn-success" ID="ButtonExcel" runat="server" Text="Export to Excel" OnClick="OnClickExportExcel"/>
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
                        	<div class="panel-heading" style="height:55px">
                                <table>
                                    <tr>
                                        <td>
                                        <asp:Label ID="Label1" Font-Bold="true" CssClass="control-label" runat="server" Text="List of Scorecard Users - Period: "></asp:Label>
                                        </td>
                                        <td>&nbsp;&nbsp;&nbsp;&nbsp;</td>
                                        <td>
                                        <!-- <asp:DropDownList ID="DropDownListPeriod" Height="30px" Width="250px" CssClass="form-control" runat="server"></asp:DropDownList>-->
                                        <div class="btn-group" role="group">
                                            <asp:PlaceHolder ID="PlaceHolderPeriod" runat="server"></asp:PlaceHolder>
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
