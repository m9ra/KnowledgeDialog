function init_autocomplete() {
    var available_answers = [
     "%out_of_database%",
     "Barack Obama",
     "Michelle Obama",
     "Malia Obama, TODO"
    ];

    $(function () {
        $(".question_annotator").autocomplete({
            source: available_answers
        });
    });
}


init_autocomplete()
