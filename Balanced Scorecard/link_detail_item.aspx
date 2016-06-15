<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="link_detail_item.aspx.cs" Inherits="Balanced_Scorecard.link_detail_item" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Link Share Measures | Financial Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><a id="financial_measures_breadcrumb" runat="server">Financial Measure</a></li>
                              <li><b>Link Share Measures in <asp:Label ID="LabelBreadcrumb" runat="server" Text="Bread"></asp:Label></b></li>
    						</ol>
                        	<h1><i class="fa fa fa-link"></i>&nbsp;Link Share Measures in <asp:Label ID="LabelTitle" runat="server" Text="Title"></asp:Label></h1>
                        </div>
                    </div>
                <form id="Form1" runat="server">
                    <p style="margin-top:10px"><b>Planning Period: </b><asp:Label ID="LabelPeriod" runat="server" Text="Period"></asp:Label></p>
                    <div class="row link-row">
                        <div class="col-xs-5 col-md-5">
                            <div class="panel panel-primary">
                            	<div class="panel-heading bold-heading">Share Measures:</div>
                                <asp:ListBox ID="ListBoxLeft" Height="275px" CssClass="listbox-custom" runat="server" SelectionMode="Multiple"></asp:ListBox>
                            </div>
                        </div>
                        <div class="col-xs-2 col-md-2" style="text-align:center; top:100px">
                            <button id="Button1" class="btn btn-primary custom-arrow" runat="server" onserverclick="OnClickBtnLeftToRight"><i class="fa fa-2x fa-long-arrow-right"></i></button><br/>
                            <button id="Button2" class="btn btn-primary custom-arrow" runat="server" onserverclick="OnClickBtnRightToLeft"><i class="fa fa-2x fa-long-arrow-left"></i></button>
                        </div>
                        <div class="col-xs-5 col-md-5">
                            <p class="error-message-link" id="ErrorMessageLink" runat="server">Share measure should be more than one</p>
                            <div class="panel panel-primary" style="margin-top:-20px">
                            	<div class="panel-heading bold-heading">To Be Linked Share Measures:</div>
                                <asp:ListBox ID="ListBoxRight" Height="275px" CssClass="listbox-custom" runat="server" SelectionMode="Multiple"></asp:ListBox>
                            </div>
                        </div>
                    </div>
                      <div class="form-group">
                        <div class="col-xs-4 col-md-4"></div>
                        <div class="col-xs-3 col-md-3"></div>
                        <div class="col-xs-4 col-md-4">
                            <label for="inputLinkWeight">Link Items' Weight</label>
                        </div>
                      </div>
                    <div class="form-group">
                        <div class="col-xs-4 col-md-4">&nbsp;</div><!-- agar format tetap rapi -->
                        <div class="col-xs-3 col-md-3">&nbsp;</div>
                        <div class="col-xs-4 col-md-4">
                            <div class="input-group">
                               <input type="number" min="0" max="100" class="form-control" id="inputLinkWeight" runat="server" value="0"/>
                               <div class="input-group-addon">%</div>
                            </div>
                        </div>
                      </div>
                      <div class="form-group">
                              <div class="col-xs-7 col-sm-7">
                                  <a id="btnCancelLinking" runat="server" class="btn btn-primary btn-cancel-link"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;&nbsp;Cancel Linking</a>
                              </div>
                               <div class="col-xs-4 col-sm-4">
                                  <span class="btn btn-success btn-done-linking btn-done-linking-container done-button">
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
