$(document).ready(function(){
    $("#searchInput").keyup(function(){
        var term = $(this).val();
        $.get("/Events/Search", { term: term }, function(data){
            $("#searchResults").html(data);
        });
    });
});
