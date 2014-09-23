<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Kanban.aspx.cs" Inherits="Rally2Slack.Web.Kanban" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script type="text/javascript" src="Scripts/jquery-2.1.0.js"></script>
    <script>
        function GetURLParameter(sParam)
        {
            var sPageURL = window.location.search.substring(1);
            var sURLVariables = sPageURL.split('&');
            for (var i = 0; i < sURLVariables.length; i++) 
            {
                var sParameterName = sURLVariables[i].split('=');
                if (sParameterName[0] == sParam) 
                {
                    return sParameterName[1];
                }
            }
            return null;
        }

        $(document).ready(function() {
            var channelName = GetURLParameter("channelName");
            var response = $.ajax({
                                url: "Api/Rally/KanbanData/" + channelName,
                                dataType: "json",
                                type: "GET",
                                async: false
            }).responseText;
            var data = JSON.parse(response);
            //$("#response").text = response;
            //console.log(response);
            //console.log(data);

            $.each(data.KanbanItems, function (i, v) {
                var key = v.Key;
                var value = v.Value;
                if (key == "") {
                    key = "None";
                }
                $("#board").append("<div style='float:left;' id='KanbanColumn_" + i + "'>" + "<div style='text-align:center'>" + key + "</div></div>");
                //console.log(data.KanbanItems);
                $.each(value, function (j, u) {
                    var itemDivId = "KanbanItem_" + i + "_" + j;
                    var itemDivIdInner = "KanbanItem_" + i + "_" + j+"_inner";
                    $("#KanbanColumn_" + i).append("<div style='margin:20px;width:250px;border: 2px solid;' id='" + itemDivId + "'></div>");
                    $("#" + itemDivId).append("<div style='height:10px;background-color: #3B89F6;color:#3B89F6;'>" + "</div>");
                    $("#" + itemDivId).append("<div style='padding:10px' id='" + itemDivIdInner + "'></div>");
                    $("#" + itemDivIdInner).append("<div style='position: relative;float:left;color:#3B89F6;' id='" + itemDivId + "'_Formatted>" + u.FormattedID + "</div>");
                    $("#" + itemDivIdInner).append("<div style='position: relative;float:right;' id='" + itemDivId + "'_Owner>" + u.Owner + "</div>");
                    $("#" + itemDivIdInner).append("<div style='clear:both' id='" + itemDivId + "'_KanbanState>" + u.KanbanProgress + "</div>");
                    $("#" + itemDivIdInner).append("<div id='" + itemDivId + "'_Name>" + u.Name + "</div>");

                    
                    //$("#KanbanColumn_" + i).append("<div id='" + i + "_" + j + "'>" + u.FormattedID + "</div>");
                });

            });
        });
    </script>
    
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="board">
        
    </div>
    </form>
</body>
</html>
