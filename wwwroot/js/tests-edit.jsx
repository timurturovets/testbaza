class EditableTest extends React.Component {
    constructor(props) {
        super(props);

        this.populateData = this.populateData.bind(this);
        this.renderTest = this.renderTest.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handleAddQuestion = this.handleAddQuestion.bind(this);
        this.onQuestionDeleted = this.onQuestionDeleted.bind(this);
        this.handlePublish = this.handlePublish.bind(this);
        this.handleUnsavedChange = this.handleUnsavedChange.bind(this);
        this.state = {
            isLoading: true,
            test: {},
            isChanged: false,
            success: false,
            isSaved: true,
            hasQuestions: false,
            publishingErrors: []
        };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const { isLoading, test, isChanged, success, isSaved } = this.state;
        const content = isLoading
            ? <h1 className="display-6">Загрузка...</h1>
            : this.renderTest()
        return (<div>
            {isChanged
                ? success
                    ? <div className="text-success"><h5 className="display-5">Изменения сохранены</h5></div>
                    : <div className="text-danger"><h5 className="display-5">Произошла ошибка. Попробуйте снова</h5></div>
                : <div></div>
            }
            {content}
        </div>);
    }

    async populateData() {
        const id = this.props.testId;
        await fetch(`/tests/get-test${id}`).then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                console.log(result);
                this.setState({
                    isLoading: false,
                    test: result,
                    hasQuestions: result.questions.length > 0
                });
            } else {
                window.location.replace('/home/index');
            }
        });
    }

    renderTest() {
        const test = this.state.test;
        const name = test.testName,
            description = test.description,
            questions = test.questions,
            isPrivate = test.isPrivate,
            hasQuestions = this.state.hasQuestions,
            publishingErrors = this.state.publishingErrors,
            isSaved = this.state.isSaved;

        return (<div>
            <form name="edit-test" className="form-horizontal">
                <div className="form-group">
                    <label className="display-6">Название теста</label>
                    <input type="text" className="form-control" name="testName"
                        onChange={this.handleUnsavedChange} defaultValue={name} />
                </div>
                <div className="form-group">
                    <label className="display-6">Описание теста</label>
                    <input type="text" className="form-control" name="description"
                        onChange={this.handleUnsavedChange} defaultValue={description} />
                </div>
                <div className="form-group">
                    <div className="form-check form-switch">
                        {isPrivate
                            ? <input className="form-check-input" type="checkbox" name="isprivate"
                                onClick={this.handleUnsavedChange} defaultChecked />
                            : <input className="form-check-input" type="checkbox" name="isprivate"
                                onClick={this.handleUnsavedChange} />}
                        <label className="form-check-label">Доступ только по ссылке</label>
                    </div>
                </div>
                {isSaved
                    ? <button className="btn btn-outline-success" onClick={e => this.handleSubmit(e)} disabled>Сохранить изменения</button>
                    : <button className="btn btn-outline-success" onClick={e => this.handleSubmit(e)}>Сохранить изменения</button>
                }
            </form>
            <h1 className="text-center display-3">Вопросы в тесте</h1>
            {console.log(questions)}
            {hasQuestions
                ? questions.map(question =>
                    <div key={question.questionId}>
                        <EditableQuestion
                            questionId={question.questionId}
                            number={question.number}
                            value={question.value}
                            hint={question.hint}
                            hintEnabled={question.hintEnabled}
                            answer={question.answer}
                            answers={question.answers}
                            answerType={question.answerType}
                            onDeleted={this.onQuestionDeleted}
                            onSavedChange={this.props.onSavedChange}
                        />
                    </div>
                )
                : <div><p>В тесте отсутствуют вопросы.</p></div>}
            <hr />
            <div className="form-group">
                <button className="btn btn-outline-success" onClick={e => this.handleAddQuestion(e)}>Добавить вопрос</button>
            </div>
            <div className="text-center">

                {publishingErrors.length > 0
                    ? publishingErrors.map(error =>
                        <div key={error} className="text-center">
                            <h4 className="text-danger">{error}</h4>
                            <hr />
                        </div>)
                    : null}
            </div>
            <div className="text-center">
                <button className="btn btn-outline-primary" onClick={e => this.handlePublish(e)}>
                    <h3 className="display-3">Опубликовать тест</h3>
                </button>
            </div>
        </div>);
    }

    async handleSubmit() {
        event.preventDefault();
        const id = this.props.testId;
        const form = document.forms["edit-test"];
        if (form.elements["testName"].value === "" || form.elements["description"].value === "") {
            alert("Вы не можете оставить поля формы пустыми");
            return;
        }
        const isPrivate = form.elements["isprivate"].checked;
        const formData = new FormData(form);
        formData.append('testid', id);
        formData.set('isprivate', isPrivate)
        await fetch('/tests/update-test', {
            method: 'PUT',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const result = await response.json();

                this.setState({ test: result, isChanged: true, success: true, isSaved: true });

            } else this.setState({ isChanged: true, success: false, isSaved: false });
        });
    }

    async handleAddQuestion(e) {
        e.preventDefault();
        const id = this.props.testId;
        const formData = new FormData();
        formData.append('testId', id);
        await fetch('/tests/add-question', {
            method: 'PUT',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                console.log(result);

                const questionId = result.questionId,
                    questionNumber = parseInt(result.number);

                const updatedTest = this.state.test;
                updatedTest.questions.push({ questionId: questionId, number: questionNumber, value: "", answer: "" });

                this.setState({ test: updatedTest, hasQuestions: true });

            } else console.log(`status: ${response.status}`);
        });
    }

    async onQuestionDeleted(id, number) {
        const test = this.state.test;
        const formData = new FormData();
        formData.append('questionId', id);
        await fetch(`/tests/delete-question`, {
            method: 'POST',
            body: formData
        }).then(response => {
            if (response.status === 200) {
                for (const obj of test.questions) {
                    if (obj.questionId === id) {
                        test.questions.splice(test.questions.indexOf(obj), 1);
                        break;
                    }
                }
                for (const obj of test.questions) {
                    if (obj.number < number) continue;
                    obj.number--;
                }
                const anyQuestionsLeft = test.questions.length > 0;
                this.setState({ test: test, hasQuestions: anyQuestionsLeft });
            } else {
                window.location.replace('/home/index');
            }
        });
    }

    async handlePublish(event) {
        event.preventDefault();
        const id = this.props.testId;

        await fetch(`/tests/publish-test${id}`).then(async response => {
            if (response.status === 200) {
                window.location.href = "/profile";
            } else {
                const result = await response.json();
                const errors = result.errors;
                this.setState({ publishingErrors: errors });
            }
        });
    }

    handleUnsavedChange() {
        this.props.onSavedChange(false);
        this.setState({ isSaved: false });
    }
}

class EditableQuestion extends React.Component {
    constructor(props) {
        super(props);

        this.handleAnswerTypeChange = this.handleAnswerTypeChange.bind(this);
        this.handleUnsavedState = this.handleUnsavedState.bind(this);
        this.handleDelete = this.handleDelete.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handleAddAnswer = this.handleAddAnswer.bind(this);
        this.onAnswerValueChange = this.onAnswerValueChange.bind(this);
        this.onAnswerDelete = this.onAnswerDelete.bind(this);

        this.state = {
            changed: false,
            success: false,
            saved: true,
            value: this.props.value === null ? "" : this.props.value,
            hint: this.props.hint === null ? "" : this.props.hint,
            hintEnabled: this.props.hintEnabled === null ? false : this.props.hintEnabled,
            answer: this.props.answer === null ? "" : this.props.answer,
            answers: this.props.answers === null || this.props.answers === undefined ? [] : this.props.answers,
            answerType: this.props.answerType === null ? 1 : this.props.answerType
        };
    }

    render() {
        const { changed, success, saved, value, hint, hintEnabled, answer, answers, answerType } = this.state;
        return <div>
            <hr />
            {changed
                ? (success
                    ? <div className="text-success"><h6 className="display-6">Изменения успешно сохранены</h6></div>
                    : <div className="text-danger"><h6 className="display-6">Ошибка. Попробуйте ещё раз</h6></div>)
                : <div></div>}
            <form name={`edit-question${this.props.questionId}`}>
                <h2>Вопрос {this.props.number} {saved ? null : "*"}</h2>

                <div className="form-check form-switch">
                    {answerType === 1
                        ? <input type="radio" className="form-check-input" name="model.AnswerType" value="1"
                            onClick={e => this.handleAnswerTypeChange(e)} defaultChecked />
                        : <input type="radio" className="form-check-input" name="model.AnswerType" value="1"
                            onClick={e => this.handleAnswerTypeChange(e)} />
                    }
                    <label className="form-check-label">Ответ вводится пользователем</label>
                </div>

                <div className="form-check form-switch">
                    {answerType === 2
                        ? <input type="radio" className="form-check-input" name="model.AnswerType" value="2"
                            onClick={e => this.handleAnswerTypeChange(e)} defaultChecked />
                        : <input type="radio" className="form-check-input" name="model.AnswerType" value="2"
                            onClick={e => this.handleAnswerTypeChange(e)} />
                    }
                    <label className="form-check-label">Несколько вариантов ответа</label>
                </div>

                <div className="form-group">
                    <label>Вопрос:</label>
                    <input type="text" className="form-control" onChange={this.handleUnsavedState} name="model.Value" defaultValue={value} />
                </div>
                {hintEnabled
                    ? <div><div className="form-check form-switch">
                        <input type="checkbox" className="form-check-input" name="model.HintEnabled" defaultChecked
                            onClick={e=>this.handleHintPresence(e)} />
                        <label className="form-check-label">Подсказка</label>
                        </div>
                    <div className="form-group">
                            <input type="text" className="form-control" onChange={this.handleUnsavedState}
                                name="model.Hint" defaultValue={hint === null ? "" : hint} />
                    </div></div>
                    : <div className="form-check form-switch">
                        <input type="checkbox" className="form-check-input" name="model.HintEnabled"
                        onClick={e => this.handleHintPresence(e)} />
                        <label className="form-check-label">Подсказка</label>
                    </div>
                }
                {this.state.answerType === 2
                    ? <div className="form-group">
                        <label>Верные ответы:</label>
                    </div>
                    : null}
                {this.state.answerType === 1
                    ? <div className="form-group">
                        <label>Верный ответ:</label>
                        <input type="text" className="form-control" onChange={this.handleUnsavedState}
                            defaultValue={answer} name="model.Answer" />
                    </div>
                    : answers.map(answer =>
                        <div key={answer.answerId} className="form-group">
                            <EditableAnswer
                                onValueChange={this.onAnswerValueChange}
                                onDelete={this.onAnswerDelete}
                                value={answer.value}
                                number={answer.number}
                                answerId={answer.answerId} />
                        </div>)
                }
                {this.state.answerType === 2
                    ? <div className="form-group">
                        <button className="btn btn-outline-success" onClick={e => this.handleAddAnswer(e)}>Добавить</button>
                    </div>
                    : null
                }
                <div className="btn-toolbar">
                    <div className="btn-group mr-2">
                        {saved
                            ? <button className="btn btn-outline-success" onClick={e => this.handleSubmit(e)} disabled>Сохранить изменения</button>
                            : <button className="btn btn-outline-success" onClick={e => this.handleSubmit(e)}>Сохранить изменения</button>
                        }
                        <button className="btn btn-outline-danger" onClick={e => this.handleDelete(e)}>Удалить вопрос</button>
                    </div>
                </div>
            </form>
        </div>
    }

    handleAnswerTypeChange(event) {
        const elem = event.target;
        this.props.onSavedChange(false);
        this.setState({ answerType: parseInt(elem.value), saved: false });
    }

    handleHintPresence(event) {
        const elem = event.target;
        this.props.onSavedChange(false);
        this.setState({hintEnabled: elem.checked, saved: false})
    }
    handleUnsavedState() {
        this.setState({ saved: false });
        this.props.onSavedChange(false);
    }
    async handleSubmit(e) {
        this.setState({ saved: true });
        this.props.onSavedChange(true);
        e.preventDefault();
        
        const id = this.props.questionId;
        const answers = this.state.answers;

        const form = document.forms[`edit-question${id}`];

        const hintEnabled = form.elements["model.HintEnabled"].checked;
        const formData = new FormData(form);

        formData.append('model.QuestionId', id);
        formData.set('model.HintEnabled', hintEnabled);

        console.log(answers);
        await fetch('/tests/update-question', {
            method: 'PUT',
            body: formData
        }).then(response => {
            this.setState({ changed: true, success: response.status === 200 });
        })
    }

    async handleDelete(event) {
        event.preventDefault();
        const id = this.props.questionId;
        this.props.onDeleted(id, this.props.number);
    }

    onAnswerValueChange(event, id) {
        console.log(`Answer value change event, id: ${id}`);
        const value = event.target.value;
        const answers = this.state.answers;
        for (const obj of answers) {
            if (obj.answerId === id) {
                console.log(obj);
                answers[answers.indexOf(obj)].value = value;
                console.log(`value: ${obj.value}`);
                this.setState({ answers: answers, saved: false });
                this.props.onSavedChange(false);
                break;
            }
        }
    }

    async onAnswerDelete(event, answerId) {
        event.preventDefault();

        const questionId = this.props.questionId;

        const formData = new FormData();
        formData.append('answerId', answerId);
        formData.append('questionId', questionId);

        await fetch("/tests/delete-answer", {
            method: "POST",
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const answers = this.state.answers;
                for (const obj of answers) {
                    if (obj.answerId === answerId) {
                        answers.splice(answers.indexOf(obj), 1);
                        break;
                    }
                }
                this.setState({ answers: answers });
            }
        });
    }

    async handleAddAnswer(event) {
        event.preventDefault();
        const id = this.props.questionId;
        console.log(`questionID: ${id}`);
        const formData = new FormData();
        formData.append("questionId", id);
        await fetch("/tests/add-answer", {
            method: "POST",
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                console.log(result);

                const answers = this.state.answers;
                answers.push({ answerId: result.answerId, number: result.number, value: "" });

                this.setState({ answers: answers });

            } else {
                console.error(`status: ${response.status}`);
            }
        });
    }
}

class EditableAnswer extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const id = this.props.answerId,
            value = this.props.value,
            number = this.props.number;
        console.log('number: ' + number);
        return (<div className="input-group mb-3">
            <input type="text" name={`model.Answers[${number - 1}].Value`} className="form-control" onBlur={e => this.props.onValueChange(e, id)}
                defaultValue={value} />
            <div className="input-group-append">
                <button className="btn btn-outline-danger" onClick={e => this.props.onDelete(e, id)}>Удалить</button>
            </div>
            <input type="hidden" name={`model.Answers[${number - 1}].AnswerId`} defaultValue={id} />
            <input type="hidden" name={`model.Answers[${number - 1}].Number`} defaultValue={number} />
        </div>)
    }
}
