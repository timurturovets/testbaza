class EditableAnswer extends React.Component {
    constructor(props) {
        super(props);
        this.state = { deleted: false };
    }

    render() {
        if (this.state.deleted) return <div></div>;
        const id = this.props.id,
            value = this.props.value; 
        return
            <div className="form-group">
                <input type="text" name="answers" className="form-control" onChange={e => this.props.onValueChange(e, id)} defaultValue={value} />
            </div>
    }

}

class EditableQuestion extends React.Component {
    constructor(props) {
        super(props);

        this.handleAnswerTypeChange = this.handleAnswerTypeChange.bind(this);
        this.handleDelete = this.handleDelete.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.onAnswerValueChange = this.onAnswerValueChange.bind(this);

        this.state = {
            deleted: false,
            changed: false,
            success: false,
            answerType: this.props.answerType,
            answers: []
        };
    }

    render() {
        const value = this.props.value === null ? "" : this.props.value,
            answer = this.props.answer === null ? "" : this.props.answer,
            answers = this.props.answers === null || this.props.answers === undefined ? [] : this.props.answers,
            answerType = this.props.answerType === null ? 1 : this.props.answerType;
        return this.state.deleted
            ? <div></div>
            : <div>
                <hr />
                {this.state.changed
                    ? (this.state.success
                        ? <div className="text-success"><h6 className="display-6">Изменения успешно сохранены</h6></div>
                        : <div className="text-danger"><h6 className="display-6">Ошибка. Попробуйте ещё раз</h6></div>)
                    : <div></div>}
                <form name={`edit-question${this.props.id}`}>
                    <h2>Вопрос {this.props.number}</h2>

                    <div className="form-check form-switch">
                        { answerType === 1
                            ? <input type="radio" className="form-check-input" name="answertype" value="1"
                                onClick={e => this.handleAnswerTypeChange(e)} defaultChecked />
                            : <input type="radio" className="form-check-input" name="answertype" value="1"
                                onClick={e => this.handleAnswerTypeChange(e)} />
                            }
                        <label className="form-check-label">Ответ вводится пользователем</label>
                    </div>

                    <div className="form-check form-switch">
                        {answerType === 2
                            ? <input type="radio" className="form-check-input" name="answertype" value="2"
                                onClick={e => this.handleAnswerTypeChange(e)} defaultChecked/>
                            : <input type="radio" className="form-check-input" name="answertype" value="2"
                                onClick={e => this.handleAnswerTypeChange(e)} />
                        }
                        <label className="form-check-label">Несколько вариантов ответа</label>
                    </div>

                    <div className="form-group">
                        <label>Вопрос:</label>
                        <input type="text" className="form-control" defaultValue={value} name="value" />
                    </div>

                    {this.state.answerType === 1
                        ? <div className="form-group">
                            <label>Верный ответ:</label>
                            <input type="text" className="form-control" defaultValue={answer} name="answer" />
                        </div>
                        : answers.map(answer =>
                            <div key={answer.id} className="form-group">
                                <EditableAnswer onValueChange={this.onAnswerValueChange} value={answer.value} />
                            </div>)
                    }
                    <div className="btn-toolbar">
                        <div className="btn-group mr-2">
                            <button className="btn btn-outline-success" onClick={e => this.handleSubmit(e)}>Сохранить изменения</button>
                            <button className="btn btn-outline-danger" onClick={e => this.handleDelete(e)}>Удалить вопрос</button>
                        </div>
                    </div>
                </form>
            </div>
    }
    handleAnswerTypeChange(event) {
        const elem = event.target;
        this.setState({ answerType: parseInt(elem.value) });
    }
    async handleSubmit(e) {
        e.preventDefault();
        const id = this.props.id;
        const form = document.forms[`edit-question${id}`];
        if (form.elements["value"].value === "" || form.elements["answer"] === "") {
            alert('Вы не можете оставить поля формы пустыми');
            return;
        }
        const formData = new FormData(form);
        formData.append('id', id);
        await fetch('/tests/update-question', {
            method: 'PUT',
            body: formData
        }).then(response => {
            this.setState({ changed: true, success: response.status === 200 ? true : false });
        })
    }

    async handleDelete(event) {
        event.preventDefault();
        const id = this.props.id;
        this.props.onDeleted(this.props.number);
        const formData = new FormData();
        formData.append('id', id);
        await fetch(`/tests/delete-question`, {
            method: 'POST',
            body: formData
        }).then(response => {
            if (response.status === 200) {
                this.setState({ deleted: true })
            } else {
               // window.location.replace('/home/index');
            }
        });
    }
    onAnswerValueChange(event, id) {
        event.preventDefault();
        const value = event.target.value;
        const answers = this.state.answers;
        for (const obj of answers) {
            if (obj.id === id) {
                answers[answers.indexOf(obj)].value = value;
                this.setState({answers: answers})
                break;
            }
        }
    }
}

class EditableTest extends React.Component {
    constructor(props) {
        super(props);

        this.populateData = this.populateData.bind(this);
        this.renderTest = this.renderTest.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handleAddQuestion = this.handleAddQuestion.bind(this);
        this.handleEnumerationChange = this.handleEnumerationChange.bind(this);

        this.state = {isLoading: true, test: {}, isChanged: false, success: false };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const content = this.state.isLoading
            ? <h1 className="display-6">Загрузка...</h1>
            : this.renderTest()
        return (<div>
            {this.state.isChanged
                ? this.state.success
                    ? <div className="text-success"><h5 className="display-5">Изменения сохранены</h5></div>
                    : <div className="text-danger"><h5 className="display-5">Произошла ошибка. Попробуйте снова</h5></div>
                : <div></div>
            }
            {content}
            </div>);
    }

    async populateData() {
        const id = this.props.id;
        await fetch(`/tests/get-test${id}`).then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                console.log(result);
                this.setState({
                    isLoading: false,
                    test: result
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
            isPrivate = test.isPrivate;

        return (<div>
            <form name="edit-test" className="form-horizontal">
                <div className="form-group">
                <label className="display-6">Название теста</label>
                    <input type="text" className="form-control" name="testName" defaultValue={name} />
                </div>
                <div className="form-group">
                <label className="display-6">Описание теста</label>
                    <input type="text" className="form-control" name="description" defaultValue={description} />
                </div>
                <div className="form-group">
                    <div className="form-check form-switch">
                    {isPrivate
                        ? <input className="form-check-input" type="checkbox" name="isprivate" defaultChecked />
                        : <input className="form-check-input" type="checkbox" name="isprivate" />}
                        <label className="form-check-label">Доступ только по ссылке</label>
                        </div>
                </div>
                <button className="btn btn-outline-success" onClick={e=>this.handleSubmit(e)}>Сохранить изменения</button>
            </form>
            <h1 className="text-center display-3">Вопросы в тесте</h1>
            {console.log(questions)}
                {questions.length > 0
                ? questions.map(question =>
                    <div key={question.id}>
                        <EditableQuestion
                            id={question.id}
                            number={question.number}
                            value={question.value}
                            answerType={question.answerType}
                            answer={question.answer}
                            answers={question.answers}
                            onDeleted={this.handleEnumerationChange}
                        />
                    </div>
                )
                : <div><p>В тесте отсутствуют вопросы.</p></div>}
            <div className="form-group">
                <button className="btn btn-outline-success" onClick={e => this.handleAddQuestion(e)}>Добавить вопрос</button>
            </div>
        </div>);
    }

    async handleSubmit() {
        event.preventDefault();
        const id = this.props.id;
        const form = document.forms["edit-test"];
        if (form.elements["testName"].value === "" || form.elements["description"].value === "") {
            alert("Вы не можете оставить поля формы пустыми");
            return;
        }
        const isPrivate = form.elements["isprivate"].checked;
        const formData = new FormData(form);
        formData.append('id', id);
        formData.set('isprivate', isPrivate)
        await fetch('/tests/update-test', {
            method: 'PUT',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const result = await response.json();

                this.setState({ test: result, isChanged: true, success: true });

            } else this.setState({ isChanged: true, success: false });
        });
    }

    async handleAddQuestion(e) {
        e.preventDefault();
        const id = this.props.id;
        const formData = new FormData();
        formData.append('testId', id);
        await fetch('/tests/add-question', {
            method: 'PUT',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                console.log(result);

                const questionId = result.id,
                    questionNumber = parseInt(result.number);

                const updatedTest = this.state.test;
                updatedTest.questions.push({ id: questionId, number: questionNumber, value: "", answer: "" });

                this.setState({ test: updatedTest });

            } else console.log(`status: ${response.status}`);
        });
    }

    handleEnumerationChange(number) {
        const test = this.state.test;
        for (const question of test.questions) {
            if (question.number < number + 1) continue;
                question.number--;
        }
        this.setState({ test: test });
    }
}