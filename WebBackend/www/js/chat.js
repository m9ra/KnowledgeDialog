function init_chat_controls() {
    $(".utterance_submit").on('click', chat_submit);
    $(".utterance").keypress(function (e) {
        if (e.which == 13) {
            //on enter
            return chat_submit();
        }
    });
}

function scroll_chat_bottom() {
    var log = $(".dialog_log");
    log.animate({ scrollTop: log[0].scrollHeight }, "slow");
    return false;
}

function chat_submit() {
    var utterance = $(".utterance");
    var text = utterance.val();
   
    var result = $.get("/dialog_data", { "utterance": text }, function (data) {
        $(".dialog_log").html(data);
        scroll_chat_bottom();
    })

    utterance.val("");
    return false;
}

init_chat_controls();
scroll_chat_bottom();
