window.addEventListener("load", function () {
    const a = document.querySelector("a.PostLogoutRedirectUri");
    const label = document.getElementById("count");

    if (a) {
        setTimeout(function (){
            const value = label.textContent;
            var index = parseInt(value);
            setInterval(function(){
                index--;
                label.innerHTML = index.toString();
            }, 1000)
        }, 1000);

        const value = label.textContent;
        setTimeout(function (){window.location = a.href;}, parseInt(value) * 1000) 
    }
});
