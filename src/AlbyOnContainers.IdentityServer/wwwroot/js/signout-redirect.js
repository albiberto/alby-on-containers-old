window.addEventListener("load", function () {
    const a = document.querySelector("a.PostLogoutRedirectUri");
    const label = document.getElementById("count");

    if (a) {
        const value = label.textContent;
        var index = parseInt(value);
        setInterval(function(){
            index--;
            label.innerHTML = index.toString();
            
            if(index === -1){
                window.location = a.href
            }
        }, 1000)
    }
});
