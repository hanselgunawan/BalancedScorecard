<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="user_access_rights.aspx.cs" Inherits="Balanced_Scorecard.user_access_rights" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    User Access Rights | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <form id="form1" runat="server">
    <script type = "text/javascript">

        //Source: www.aspsnippets.com/Articles/Implement-check-all-checkbox-functionality-in-ASPNet-GridView-control-using-JavaScript.aspx
        function Check_Click(objRef) {
            var row = objRef.parentNode.parentNode;//untuk mencapai tag <column> pada GridView, karena yang mau diliat ada kolom/column-nya
            if (objRef.checked) {
                row.style.backgroundColor = "#eee";//kalau di checked, warnanya jadi abu-abu
            }
            else {
                if (row.rowIndex % 2 == 0) {//kalau baris genap, waktu di unchecked warnanya jadi abu-abu keputihan
                    row.style.backgroundColor = "#f9f9f9";
                }
                else {
                    row.style.backgroundColor = "white";//kalau baris ganjil, waktu di unchecked warnanya jadi putih lagi
                }
            }

            var GridView = row.parentNode;//untuk mencapai tag <GridView>

            var inputList = GridView.getElementsByTagName("input");//mengambil semua tag <input> pada gridview
                                                                  //setiap checkbox memilih tag HTML = input

            for (var i = 0; i < inputList.length; i++) {//untuk mencetang header check box.
                var headerCheckBox = inputList[0];//jika semuanya di checked, header check box akan otomatis di checked juga
                                                    
                var checked = true;
                if (inputList[i].type == "checkbox" && inputList[i] != headerCheckBox) {
                    if (!inputList[i].checked) {//jika ditemukan checkbox yang tidak di checked, langsung keluar dari loop (break)
                        checked = false;       //checked berubah menjadi FALSE.
                        break;                //jika semua di checked, checked tetap TRUE.
                    }
                }
            }
            headerCheckBox.checked = checked;//untuk check/uncheck Header CheckBox
        }

        function checkAll(objRef) {
            var GridView = objRef.parentNode.parentNode.parentNode;//untuk mencapai tag <GridView>
            var inputList = GridView.getElementsByTagName("input");//mengambil semua tag <input> pada GridView
            for (var i = 0; i < inputList.length; i++) {

                var row = inputList[i].parentNode.parentNode;//untuk mencapai tag <column>
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                    if (objRef.checked) {//jika Header CheckBox di checked, semua checkbox di checked
                        row.style.backgroundColor = "#eee";
                        inputList[i].checked = true;
                    }
                    else {//jika tidak di checked, semua checkbox tidak di checked
                        if (row.rowIndex % 2 == 0) {
                            row.style.backgroundColor = "#f9f9f9";
                        }
                        else {
                            row.style.backgroundColor = "white";
                        }
                        inputList[i].checked = false;
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
                                            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:MyConnection %>" SelectCommand="SELECT [UserGroup_ID], [Group_Name] FROM [BSC_UserGroup] WHERE UserGroup_ID>1"></asp:SqlDataSource>
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
