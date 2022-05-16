class EditableTest extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            isLoading: true,
            test: {},
            isChanged: false,
            success: false,
            isSaved: true,
            hasQuestions: false,
            publishingErrors: [],
            isTimeLimited: false
        };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const content = this.state.isLoading
            ? <h1>Информация о тесте загружается...</h1>
            : this.renderTest()
        return content;
    }

    populateData = async () => {
        const id = this.props.testId;
        await fetch(`/api/tests/wq/get-test${id}`).then(async response => {
            if (response.status === 200) {
                const object = await response.json();
                const test = object.result;
                this.setState({
                    isLoading: false,
                    test: test,
                    hasQuestions: test.questions.length > 0,
                    isTimeLimited: test.timeInfo.isTimeLimited
                });
            } else {
                window.location.replace('/home/index');
            }
        });
    }

    renderTest = () => {
        const { test, isChanged, success, isSaved, hasQuestions, publishingErrors } = this.state;

        const name = test.testName,
            {description, 
                questions,
                isPrivate,
                areAnswersManuallyChecked,
                timeInfo,
                allowedAttempts} = test;
            
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
                        <input className="form-check-input" type="checkbox" name="isprivate"
                            onClick={this.handleUnsavedChange} defaultChecked={isPrivate} />
                        <label className="form-check-label">Доступ только по ссылке</label>
                    </div>
                    <div className="form-check form-switch">
                        <input className="form-check-input" type="checkbox" name="areanswersmanuallychecked"
                            onClick={e => {
                                const { test } = this.state;
                                this.setState({ test: {...test, areAnswersManuallyChecked: !test.areAnswersManuallyChecked}})
                                this.handleUnsavedChange(e);
                            }} defaultChecked={areAnswersManuallyChecked} />
                        <label className="form-check-label">Проверять ответы вручную</label>
                        {areAnswersManuallyChecked
                            ? <p className="m-0 text-sm">Вы должны будете сами проверять ответы тех, кто прошёл тест.
                                Это можно будет сделать в личном кабинете.</p>
                            : <p className="m-0 text-sm">Ответы будут проверяться автоматически. Важно: <i>ответы будут проверяться на полное
                                совпадение, за исключением регистра букв. Постарайтесь максимально сузить круг
                                возможных ответов на вопросы в тесте.
                            </i></p>
                        }
                    </div>
                    <div className="form-check form-switch">
                        <label className="form-check-label">Ограничен по времени</label>
                            <input className="form-check-input" type="checkbox" name="timeinfo.istimelimited"
                                onClick={e => this.handleTimeInfoChange(e)} defaultChecked={timeInfo.isTimeLimited} />
                            {timeInfo.isTimeLimited
                                ? this.renderTimeInputs(timeInfo.hours, timeInfo.minutes, timeInfo.seconds)
                                : null }
                    </div>
                    <div className="form-check form-switch">
                        <label className="form-check-label">
                            {test.areAttemptsLimited
                                ? "Разрешённое количество попыток: "
                                : "Ограничить количество попыток"
                            }
                        </label>
                        <input className="form-check-input" type="checkbox" name="areattemptslimited"
                            onClick={e => this.handleAttemptsInfoChange(e)} defaultChecked={test.areAttemptsLimited} />
                        {test.areAttemptsLimited
                            ? <input type="number" name="allowedattempts" min={1} max={25} defaultValue={allowedAttempts}
                                onChange={e => this.handleAttemptsInfoChange(e)} />
                            : null }
                    </div>
                </div>

                <button className="btn btn-outline-success" onClick={e => this.handleSubmit(e)} disabled={isSaved}>Сохранить изменения</button>
                {isChanged
                    ? isSaved
                        ? success
                            ? <div className="text-success"><p>Изменения сохранены</p></div>
                            : <div className="text-danger"><p>Произошла ошибка. Попробуйте снова</p></div>
                        : null
                    : null
                }
            </form>
            <h1 className="text-center display-3">Вопросы в тесте</h1>
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
                            correctAnswer={question.correctAnswerNumber}
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
                <br />
                <button className="btn btn-outline-danger mt-2" onClick={e => this.handleDelete(e)}>
                    <h4 className="display-4">Удалить тест</h4>
                </button>
            </div>
        </div>);
    }

    renderTimeInputs = (hours = 0, minutes = 0, seconds = 0) => {
        return (<div>
            <input className="d-inline" type="number" name="timeinfo.hours" min="0" max="48" defaultValue={hours}
                onChange={e=>this.handleTimeInfoChange(e)}/>
            <label className="d-inline">ч</label>
            <input className="d-inline" type="number" name="timeinfo.minutes" min="0" max="59" defaultValue={minutes}
                onChange={e => this.handleTimeInfoChange(e)} />
            <label className="d-inline">мин</label>
            <input className="d-inline" type="number" name="timeinfo.seconds" min="0" max="59" defaultValue={seconds}
                onChange={e => this.handleTimeInfoChange(e)} />
            <label>сек</label>
        </div>)
    }

    handleTimeInfoChange = event => {
        const name = event.target.name.toLowerCase(),
            test = this.state.test,
            value = parseInt(event.target.value);
        if (name === "timeinfo.istimelimited") {

            test.timeInfo.isTimeLimited = event.target.checked;
            this.setState({ test: test, isSaved: false });

        } else if (name === "timeinfo.hours") {

            test.timeInfo.hours = value;
            this.setState({ test: test, isSaved: false });

        } else if (name === "timeinfo.minutes") {

            test.timeInfo.minutes = value;
            this.setState({ test: test, isSaved: false });

        } else if (name === "timeinfo.seconds") {

            test.timeInfo.seconds = value;
            this.setState({ test: test, isSaved: false });

        }
    }

    handleAttemptsInfoChange = event => {
        const name = event.target.name,
            test = this.state.test;
        if (name === 'areattemptslimited') {
            this.setState({
                test: {
                    ...test,
                    areAttemptsLimited: event.target.checked
                },
                isSaved: false
            });
        }
        if (name === 'allowedattempts') {
            this.setState({
                test: {
                    ...test,
                    allowedAttempts: event.target.value
                },
                isSaved: false
            });
        }
    }

    handleSubmit = async () => {
        const id = this.props.testId;
        const form = document.forms["edit-test"];
        if (form.elements["testName"].value === "" || form.elements["description"].value === "") {
            alert("Вы не можете оставить поля формы пустыми");
            return;
        }
        const isPrivate = form.elements["isprivate"].checked;
        const isTimeLimited = form.elements["timeinfo.istimelimited"].checked;
        const areAttemptsLimited = form.elements["areattemptslimited"].checked;
        const areAnswersManuallyChecked = form.elements["areanswersmanuallychecked"].checked;

        const formData = new FormData(form);
        formData.append('testid', id);
        formData.set('isprivate', isPrivate);
        formData.set('timeinfo.istimelimited', isTimeLimited);
        formData.set('areattemptslimited', areAttemptsLimited);
        formData.set('areanswersmanuallychecked', areAnswersManuallyChecked);

        await fetch('/api/tests/wq/update-test', {
            method: 'PUT',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const object = await response.json();
                const test = object.result;
                this.setState({ test: test, isChanged: true, success: true, isSaved: true });

            } else this.setState({ isChanged: true, success: false, isSaved: false });
        });
    }

    handleDelete = async event => {
        event.preventDefault();

        if (!confirm("Вы уверены, что хотите удалить тест? Его нельзя будет восстановить")) return;
        const id = this.props.testId;
        const formData = new FormData();
        formData.append('testId', id);
        await fetch('/api/tests/delete-test', {
            method: 'POST',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                alert('Тест успешно удалён. Переадресация на главную страницу...');
                window.location.href = "/home/index";
            } else alert(`При попытке удалить тест был получен статус ${response.status}. Попробуйте перезагрузить страницу`);
        });
    }

    handleAddQuestion = async event => {
        event.preventDefault();
        const id = this.props.testId;
        const formData = new FormData();
        formData.append('testId', id);
        await fetch('/api/tests/add-question', {
            method: 'PUT',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const object = await response.json();
                const question = object.result;
                const questionId = question.questionId,
                    questionNumber = parseInt(question.number);

                const test = this.state.test;
                test.questions.push({ questionId: questionId, number: questionNumber, value: "", answer: "" });

                this.setState({ test: test, hasQuestions: true });

            } else alert(`Произошла непредвиденная ошибка. Попробуйте снова. ${response.status}`);
        });
    }

    onQuestionDeleted = async (id, number) => {
        const test = this.state.test;
        const formData = new FormData();
        formData.append('questionId', id);
        await fetch(`/api/tests/delete-question`, {
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

    handlePublish = async event => {
        event.preventDefault();
        const id = this.props.testId;

        await fetch(`/api/tests/publish-test${id}`).then(async response => {
            if (response.status === 200) {
                window.location.href = "/profile#tests";
            } else {
                const object = await response.json();
                const errors = object.result;
                this.setState({ publishingErrors: errors });
            }
        });
    }

    handleUnsavedChange = () => {
        this.props.onSavedChange(false);
        this.setState({ isSaved: false });
    }
}

class EditableQuestion extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            changed: false,
            success: false,
            saved: true,
            value: !!this.props.value ? this.props.value : "",
            hint: !!this.props.hint ? this.props.hint : "",
            hintEnabled: !!this.props.hintEnabled ? this.props.hintEnabled : false,
            answer: !!this.props.answer ? this.props.answer : "",
            answers: !!this.props.answers ? this.props.answers : [],
            correctAnswer: !!this.props.correctAnswer ? this.props.correctAnswer : "",
            answerType: !!this.props.answerType ? this.props.answerType : 1
        };
    }

    render() {
        const { changed,
            success,
            saved,
            value,
            hint,
            hintEnabled,
            answer,
            answers,
            correctAnswer,
            answerType } = this.state;
        return <div>
            <hr />

            <form name={`edit-question${this.props.questionId}`}>
                <h2>Вопрос {this.props.number} {saved ? null : "*"}</h2>

                <div className="form-check form-switch">
                    <input type="radio" className="form-check-input" name="model.AnswerType" value="1"
                            onClick={e => this.handleAnswerTypeChange(e)} defaultChecked={answerType===1 && true} />
                    <label className="form-check-label">Ответ вводится пользователем</label>
                </div>

                <div className="form-check form-switch">
                    <input type="radio" className="form-check-input" name="model.AnswerType" value="2"
                            onClick={e => this.handleAnswerTypeChange(e)} defaultChecked={answerType===2 && true} />
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
                        <h6>Варианты ответа:</h6>
                    </div>
                    : null}
                {this.state.answerType === 1
                    ? <div className="form-group">
                        <h6>Верный ответ:</h6>
                        <input type="text" className="form-control" onChange={this.handleUnsavedState}
                            defaultValue={answer} name="model.Answer" />
                    </div>
                    : <div>{answers.length > 0 ? <label className="input-group-prepend">Верный</label> : null}

                       {answers.map(answer => {
                           return <div key={answer.answerId} className="form-group">
                               <EditableAnswer
                                   onValueChange={this.onAnswerValueChange}
                                   onDelete={this.onAnswerDelete}
                                   value={answer.value}
                                   number={answer.number}
                                   answerId={answer.answerId}
                                   isCorrect={correctAnswer === answer.number}
                                   onUnsaved={this.handleUnsavedState}
                               />
                           </div>
                       })}</div>
                }
                {this.state.answerType === 2
                    ? <div className="form-group">
                        <button className="btn btn-outline-success" onClick={e => this.handleAddAnswer(e)}>Добавить</button>
                    </div>
                    : null
                }
                <div className="btn-toolbar">
                    <div className="btn-group mr-2">
                        <button className="btn btn-outline-success" onClick={e => this.handleSubmit(e)} disabled={saved}>Сохранить изменения</button>
                        <button className="btn btn-outline-danger" onClick={e => this.handleDelete(e)}>Удалить вопрос</button>
                    </div>
                </div>
                {changed
                    ? saved
                        ? success
                            ? <div className="text-success"><p>Изменения сохранены</p></div>
                            : <div className="text-danger"><p>Ошибка. Попробуйте ещё раз</p></div>
                        : null
                    : null}
            </form>
        </div>
    }

    handleAnswerTypeChange = event => {
        const elem = event.target;
        this.props.onSavedChange(false);
        this.setState({ answerType: parseInt(elem.value), saved: false });
    }

    handleHintPresence = event => {
        const elem = event.target;
        this.props.onSavedChange(false);
        this.setState({hintEnabled: elem.checked, saved: false})
    }

    handleUnsavedState = () => {
        this.setState({ saved: false });
        this.props.onSavedChange(false);
    }

    handleSubmit = async event => {
        this.setState({ saved: true });
        this.props.onSavedChange(true);
        event.preventDefault();
        
        const id = this.props.questionId;
        const answers = this.state.answers;

        const form = document.forms[`edit-question${id}`];

        const hintEnabled = form.elements["model.HintEnabled"].checked;
        const formData = new FormData(form);

        formData.append('model.QuestionId', id);
        formData.set('model.HintEnabled', hintEnabled);

        await fetch('/api/tests/update-question', {
            method: 'PUT',
            body: formData
        }).then(response => {
            this.setState({ changed: true, success: response.status === 200 });
        })
    }

    handleDelete = async event => {
        event.preventDefault();
        const id = this.props.questionId;
        this.props.onDeleted(id, this.props.number);
    }

    onAnswerValueChange = (event, id) => {
        const value = event.target.value;
        const answers = this.state.answers;
        for (const obj of answers) {
            if (obj.answerId === id) {
                answers[answers.indexOf(obj)].value = value;
                this.setState({ answers: answers, saved: false });
                this.props.onSavedChange(false);
                break;
            }
        }
    }

    onAnswerDelete = async (event, answerId) => {
        event.preventDefault();

        const questionId = this.props.questionId;

        const formData = new FormData();
        formData.append('answerId', answerId);
        formData.append('questionId', questionId);

        await fetch("/api/tests/delete-answer", {
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

    handleAddAnswer = async event => {
        event.preventDefault();
        const id = this.props.questionId;
        const formData = new FormData();
        formData.append("questionId", id);
        await fetch("/api/tests/add-answer", {
            method: "POST",
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                const object = await response.json();

                const result = object.result;
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
        return (<div className="input-group mb-3">
            <div className="input-group-append form-check form-switch">
                <input type="radio" name="model.CorrectAnswerNumber" className="form-check-input"
                    value={number} onClick={e => this.props.onUnsaved(e)} defaultChecked={this.props.isCorrect && true} />
            </div>
            <input type="text" name={`model.Answers[${number - 1}].Value`} className="form-control" onChange={e => this.props.onValueChange(e, id)}
                defaultValue={value} />
            <div className="input-group-append">
                <button className="btn btn-outline-danger" onClick={e => this.props.onDelete(e, id)}>Удалить</button>
            </div>

            <input type="hidden" name={`model.Answers[${number - 1}].AnswerId`} defaultValue={id} />
            <input type="hidden" name={`model.Answers[${number - 1}].Number`} defaultValue={number} />
        </div>)
    }
}
