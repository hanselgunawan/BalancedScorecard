<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="import_user.aspx.cs" Inherits="Balanced_Scorecard.import_user" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Import User From Excel | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><a id="scorecard_user_link" runat="server">Scorecard Users</a></li>
                              <li><b>Import From Excel</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-user"></i>&nbsp;Import User From Excel</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form id="Form1" class="form-horizontal" style="margin-top:20px" runat="server">
                       <div class="form-group">
                        <label for="start_period_label" class="col-md-2 col-sm-3 control-label">Period</label>
                        <div class="col-md-5 col-sm-7" style="margin-top:7px">
                            <asp:Label ID="LabelPeriod" runat="server" Text="Period"></asp:Label>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="start_period_label" class="col-md-2 col-sm-3 control-label">File Name</label>
                        <div class="col-md-5 col-sm-7">
                            <asp:FileUpload ID="FileUploadExcel" CssClass="form-control" runat="server" />
                            <p class="error-message" id="check_file_type" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                      	<div class="col-md-offset-2 col-sm-offset-3 col-sm-10">
                            	<a runat="server" id="cancel_import_user" class="btn btn-primary btn-cancel-group"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Import&nbsp;&nbsp;</a>
                              
                              <span style="color:white; width:7em" class="btn btn-add-group btn-add-group-container add-button" id="SpanAddGroup" runat="server">
                                <i class="fa fa-fw fa-plus"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" OnClick="OnClickImport" ID="ButtonImport" CssClass="relative-to-btn-import" runat="server" Text="Import" />
                              </span>
                        </div>
                      </div>
                    </form>
                <!-- End of FORM -->
            </div>
            <!-- /.container-fluid -->

        </div>
        <!-- /#page-wrapper -->
</asp:Content>
