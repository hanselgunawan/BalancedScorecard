<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="user_access_rights.aspx.cs" Inherits="Balanced_Scorecard.user_access_rights" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    User Access Rights | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <form id="form1" runat="server">
    <script type = "text/javascript">

        function Check_Click(objRef) {
            var row = objRef.parentNode.parentNode;
            if (objRef.checked) {
                row.style.backgroundColor = "#eee";
            }
            else {
                if (row.rowIndex % 2 == 0) {
                    row.style.backgroundColor = "white";
                }
                else {
                    row.style.backgroundColor = "white";
                }
            }

            var GridView = row.parentNode;

            var inputList = GridView.getElementsByTagName("input");

            for (var i = 0; i < inputList.length; i++) {
                var headerCheckBox = inputList[0];

                var checked = true;
                if (inputList[i].type == "checkbox" && inputList[i] != headerCheckBox) {
                    if (!inputList[i].checked) {
                        checked = false;
                        break;
                    }
                }
            }
            headerCheckBox.checked = checked;

        }

        function checkAll(objRef) {
            var GridView = objRef.parentNode.parentNode.parentNode;
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {

                var row = inputList[i].parentNode.parentNode;
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                    if (objRef.checked) {

                        row.style.backgroundColor = "#eee";
                        inputList[i].checked = true;
                    }
                    else {

                        if (row.rowIndex % 2 == 0) {

                            row.style.backgroundColor = "white";
                        }
                        else {
                            row.style.backgroundColor = "white";
                        }
                        inputList[i].checked = false;
                    }
                }
            }
        }

        function MouseEvents(objRef, evt) {
            var checkbox = objRef.getElementsByTagName("input")[0];
            if (evt.type == "mouseover") {
                objRef.style.backgroundColor = "white";
            }
            else {
                if (checkbox.checked) {
                    objRef.style.backgroundColor = "white";
                }
                else if (evt.type == "mouseout") {
                    if (objRef.rowIndex % 2 == 0) {

                        objRef.style.backgroundColor = "white";
                    }
                    else {
                        objRef.style.backgroundColor = "white";
                    }

                }
            }
        }
    </script>

    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>User Management</li>
                              <li><b>User Access Rights</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-user"></i>&nbsp;User Access Rights</h1>
                        </div>
                    </div>
                <!-- /.row -->
                <div class="panel panel-primary">                           
                        <div class="panel-heading"> 
                            <i class="fa fa-user" style="font-weight:bold"></i>&nbsp;&nbsp;<b>Access Right</b><br/>                                  
                            <asp:LinkButton ID="btnsaveaccess" runat="server" CssClass="btn btn-default pull-right" style="margin-top:-28px" OnClick="btnsaveaccess_Click" ><i class="fa fa-save">&nbsp;Save Data</i></asp:LinkButton>                                  
                        </div>
                        <div class="panel-body">                                                                                         
                                <table>
                                    <tr>
                                        <td>
                                            <label for="TextBoxEmpGroup" class="col-sm-4 col-md-2 control-label">Role</label>
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="DropDownListUserGroup" runat="server" DataSourceID="SqlDataSource1" DataTextField="Group_Name" DataValueField="UserGroup_ID" OnSelectedIndexChanged="DropDownListUserGroup_SelectedIndexChanged" AutoPostBack="true" CssClass="form-control"></asp:DropDownList>
                                            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:MyConnection %>" SelectCommand="SELECT [UserGroup_ID], [Group_Name] FROM [BSC_UserGroup]"></asp:SqlDataSource>
                                        </td>
                                    </tr>
                                </table>  
                                                                                                                                     
                                <div class="col-lg-10 table-responsive" style="width:100%; text-align:center; margin-top:15px">                                                                                                                                                         
                                    <asp:GridView ID="GridView1" runat="server" 
                                        AutoGenerateColumns = "false" AllowPaging ="true" PageSize = "10" 
                                        OnPageIndexChanging = "OnPaging" class="table table-bordered table-striped yui" PagerStyle-CssClass="pagination-gridview" 
                                        PagerStyle-HorizontalAlign="Right" HeaderStyle-BackColor="#29166f">
                                        <Columns>
                                        <asp:TemplateField HeaderStyle-CssClass="centering-th2">
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="chkAll" runat="server" onclick = "checkAll(this);"
                                                    OnCheckedChanged = "CheckBox1_CheckedChanged"/>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chk" runat="server" onclick = "Check_Click(this)"
                                                OnCheckedChanged = "CheckBox1_CheckedChanged" />
                                            </ItemTemplate>
                                        </asp:TemplateField> 
                                        <asp:BoundField HeaderStyle-CssClass="centering-th2" DataField = "Access_Rights_Code" HeaderText = "Access Right Code" HeaderStyle-ForeColor="White"  >
                                            <HeaderStyle ForeColor="White" />
                                            </asp:BoundField>
                                        <asp:BoundField HeaderStyle-CssClass="centering-th2"  DataField = "Description" HeaderText = "Access Right Description" HeaderStyle-ForeColor="White" >
                                            <HeaderStyle ForeColor="White" />
                                            </asp:BoundField>
                                        </Columns>        
                                    </asp:GridView>                                                                                                                                                            
                                </div>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                        </div>                                                            
                </div>
            </div>
            <!-- /.container-fluid -->

        </div>
        <!-- /#page-wrapper -->
</form>
</asp:Content>
