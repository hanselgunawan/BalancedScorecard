<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="edit_period.aspx.cs" Inherits="Balanced_Scorecard.edit_period" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Edit BSC Period | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><a id="SetBSCPeriod" runat="server">Set BSC Period</a></li>
                              <li><b>Edit BSC Period</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-calendar"></i>&nbsp;Edit BSC Period</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form class="form-horizontal" runat="server" style="margin-top:20px">
                      <!--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                      <asp:UpdatePanel ID="UpdatePanelEditPeriod" runat="server">
                      <ContentTemplate>-->
                      <div class="form-group">
                        <label for="start_date" class="col-md-2 col-sm-3 control-label">Start Date</label>
                        <div class="col-md-3 col-sm-5">
                                <asp:TextBox ID="input_start_date" OnTextChanged="OnTextChanged_StartPeriod" AutoPostBack="true" CssClass="form-control" TextMode="Date" runat="server"></asp:TextBox>
                                <p class="period-error-message" id="start_year_error_message" runat="server">start year exists</p>
                        </div>
                        <div class="col-sm-4">
                                <p id="required_start_date" runat="server">This field is required</p>
                                <p id="invalid_start_date" runat="server">invalid date format</p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="end_date" class="col-md-2 col-sm-3 control-label">End Date</label>
                        <div class="col-md-3 col-sm-5">
                                <asp:TextBox ID="input_end_date" TextMode="Date" AutoPostBack="true" OnTextChanged="OnTextChanged_EndPeriod" CssClass="form-control" runat="server"></asp:TextBox>
                                <asp:Label ID="LabelYear" runat="server" style="color:red; font-weight:bold; width:120%; position:absolute" Visible="false" Text="end date should be the same year as start date"></asp:Label>
                                <p class="period-error-message" id="end_year_error_message" runat="server">end year exists</p>
                                <p class="period-error-message" id="less_than_error_message" runat="server">must greater than start date</p>
                        </div>
                        <div class="col-sm-4">
                                <p id="required_end_date" runat="server">This field is required</p>
                                <p id="invalid_end_date" runat="server" style="margin-top:-25px; color:red; font-weight:bold">invalid date format</p>
                        </div>
                      </div>                      
                      <div class="form-group">
                        <label for="description" class="col-md-2 col-sm-3 control-label">Description</label>
                        <div class="col-md-6 col-sm-8">
                          <textarea class="form-control" maxlength="100" runat="server" rows="3" id="description" placeholder="Type Some Description"></textarea>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectStatus" class="col-md-2 col-sm-3 control-label">Status</label>
                        <div class="col-md-2 col-sm-4">
                            <asp:DropDownList ID="DropDownListStatus"  OnSelectedIndexChanged="OnStatusChanged" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>
                        </div>
                      </div>
                      <div class="form-group">
                        <div class="col-sm-2"></div>
                        <div class="col-sm-6">
                          <p id="FailActive" runat="server">There has been an Active period</p>
                        </div>
                      </div>
                      <div class="form-group">
                      	<div class="col-md-offset-2 col-sm-offset-3 col-sm-10">
                      		<p>
                            <a class="btn btn-primary btn-activate-period" id="CancelEditPeriod" runat="server"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Edit&nbsp;&nbsp;</a>
                              <span style="color:white" id="SpanEditPeriod" runat="server">
                                <i class="fa fa-fw fa-pencil-square-o"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonEditPeriod" OnClick="OnClickEditPeriod" CssClass="relative-to-edit-btn" runat="server" Text="Edit" />
                              </span>
                            </p>
                        </div>
                      </div>
                      <!--</ContentTemplate>
                      </asp:UpdatePanel>-->
                    </form>
                <!-- End of FORM -->
            </div>
            <!-- /.container-fluid -->

        </div>
        <!-- /#page-wrapper -->
</asp:Content>
