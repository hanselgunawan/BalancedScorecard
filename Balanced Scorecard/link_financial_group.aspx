<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="link_financial_group.aspx.cs" Inherits="Balanced_Scorecard.link_financial_group" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Link Financial Group | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><b>Link Scorecard Group</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-link"></i>&nbsp;Link Scorecard Group</h1>
                        </div>
                    </div>
                <form id="Form1" runat="server">
                    <div class="row link-row">
                        <div class="form-group">
                        <label for="TextBoxGroup" class="col-xs-2 col-sm-2 control-label custom-group-name">Period</label>
                            <div class="col-xs-4 col-sm-5 col-md-3">
                                <!--<asp:DropDownList ID="DropDownList1" runat="server"></asp:DropDownList>-->
                                <asp:PlaceHolder ID="PlaceHolderPeriod" EnableViewState="true" runat="server"></asp:PlaceHolder>
                            </div>
                        </div>
                        <br/><br />
                        <div class="form-group">
                        <label for="TextBoxGroup" class="col-xs-2 col-sm-2 control-label custom-group-name">Group Name</label>
                            <div class="col-xs-5 col-sm-6 col-md-4">
                                <asp:DropDownList ID="DropDownListGroup" OnSelectedIndexChanged="OnSelectGroupName" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>
                            </div>
                        </div>
                        <br/><br/>
                        <div class="col-xs-5 col-md-5">
                            <div class="panel panel-primary">
                            	<div class="panel-heading bold-heading">Additional Groups:</div>
                                <asp:ListBox ID="ListBoxLeft" Height="275px" CssClass="listbox-custom" runat="server" SelectionMode="Multiple"></asp:ListBox>
                            </div>
                        </div>
                        <div class="col-xs-2 col-md-2" style="text-align:center; top:100px">
                            <button id="Button1" class="btn btn-primary custom-arrow" runat="server" onserverclick="OnClickBtnLeftToRight"><i class="fa fa-2x fa-long-arrow-right"></i></button><br/>
                            <button id="Button2" class="btn btn-primary custom-arrow" runat="server" onserverclick="OnClickBtnRightToLeft"><i class="fa fa-2x fa-long-arrow-left"></i></button>
                        </div>
                        <div class="col-xs-5 col-md-5">
                            <p class="error-message-link" id="ErrorMessageLink" runat="server">There is no additional group to be linked</p>
                            <div class="panel panel-primary">
                            	<div class="panel-heading bold-heading">Link Additional Groups:</div>
                                <asp:ListBox ID="ListBoxRight" Height="275px" CssClass="listbox-custom" runat="server" SelectionMode="Multiple"></asp:ListBox>
                            </div>
                        </div>
                    </div>
                      <div class="form-group">
                              <div class="col-xs-7 col-sm-7"></div>
                               <div class="col-xs-4 col-sm-4">
                                  <span style="color:white" class="btn btn-done-linking btn-done-linking-container done-button" id="SpanDone" runat="server">
                                  <i class="fa fa-fw fa-link"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonDone" CssClass="relative-to-btn-done" OnClick="OnClickDone" runat="server" Text="Done Linking" />
                                  </span>
                              </div>
                      </div>
                    </form>

            </div>
            <!-- /.container-fluid -->

        </div>
        <!-- /#page-wrapper -->
</asp:Content>
