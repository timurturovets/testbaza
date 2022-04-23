class ChecksList extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            isLoading: true,
            isBrowsingCheck: false,
            browsedCheck: {},
            checks: []
        };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const { isLoading, isBrowsingCheck, browsedCheck, checks } = this.state;
        const content = <div>
            <h2 className="display-2 text-center">Проверка тестов</h2>
            {isLoading
                ? <h2>Работы на проверку загружаются...</h2>
                : isBrowsingCheck
                    ? <CheckedTest info={browsedCheck} onBrowsingEnd={this.handleBrowseEnd} />
                    : <div>
                        {checks.length > 0
                        ? checks.map((check,i) => {
                            return <CheckSummary key={i}
                                info={check}
                                onCheckBrowsing={this.handleCheckBrowse}
                                />
                            })
                        : <h5 className="display-5">У вас нет непроверенных работ.</h5>}
                </div>
            } </div>
        return content;
    }

    populateData = async () => {
        const testId = this.props.testId;
        await fetch(`/api/profile/checks-info?testId=${testId}`).then(async response => {
            if (response.status === 200) {
                const object = await response.json();
                const result = object.result;
                this.setState({ isLoading: false, checks: result });
            } else alert(`Произошла непредвиденная ошибка. Попробуйте снова. ${response.status}`);
        });
    }

    handleCheckBrowse = async (event, attemptId) => {
        event.preventDefault();
        const testId = this.props.testId;
        const { checks } = this.state;
        const userName = checks.find(c => c.attemptId === attemptId).userName;

        await fetch(`/api/profile/get-stat?testId=${testId}&attemptId=${attemptId}`).then(async response => {
            if (response.status === 200) {
                const object = await response.json();
                const result = object.result;
                this.setState(
                    {
                        isBrowsingCheck: true,
                        browsedCheck: {
                            testId: testId,
                            attemptId: attemptId,
                            userName: userName,
                            ...result
                        }
                    });
            } else alert(`Произошла непредвиденная ошибка. Попробуйте снова. ${response.status}`);
        })
    }

    handleBrowseEnd = () => {
        this.setState({ isBrowsingCheck: false, browsedCheck: null });
    }
}

class CheckSummary extends React.Component {
    constructor(props) {
        super(props)
    }

    render() {
        const { userName, attemptId, isChecked } = this.props.info;
        return <div>
            <h3>Пользователь {userName}</h3>
            {isChecked
                ? <p className="text-success">Тест проверен</p>
                : <p className="text-danger">Работа не проверена</p>
            }
            <button className="btn btn-outline-primary" onClick={e=>this.props.onCheckBrowsing(e, attemptId)}>
                {isChecked
                    ? "Перепроверить"
                    : "Проверить"
                }
            </button>
        </div>;
    }
}

class CheckedTest extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            isSaved: true,
            correctAQNumbers: new Set(this.props.info.userAnswers
                .filter(a => a.isCorrect)
                .map(a => a.questionNumber)),
            incorrectAQNumbers: new Set(this.props.info.userAnswers
                .filter(a => !a.isCorrect)
                .map(a => a.questionNumber))
        };
    }

    render() {
        const { userName, timeUsed, userAnswers, questions } = this.props.info;
        const answers = userAnswers;
        const { isSaved, correctAQNumbers } = this.state;
        const combined = [];
        let i = 0;
        for (const question of questions) {
            i++;
            const userAnswer = answers.find(a => a.questionNumber === question.number);

            if (!!userAnswer) {

                combined.push({
                    id: i+Math.random()*1000,
                    question: question,
                    answer: {
                        value: !!userAnswer.value && userAnswer.value !== 'null'
                            ? userAnswer.value
                            : "",
                        questionNumber: question.number,
                        isCorrect: correctAQNumbers.has(question.number)
                    }
                });
            } else {

                combined.push({
                    id: i+Math.random()*1000,
                    question: question,
                    answer: {
                        value: "",
                        questionNumber: question.number,
                        isCorrect: false
                    }
                });
            }
        }

        return <div>
            <button className="btn btn-outline-primary"
                onClick={e => this.handleBack(e)}>Назад</button>
            <h2 className="display-2">Пользователь {userName}</h2>
            <p className="text-lg">Потратил времени на прохождение: <b>{timeUsed}</b></p>
            {combined.map(c => {
                return <div key={c.id}>
                    <hr />
                    <h4>Вопрос {c.question.number}</h4>
                    <h5 className="display-5">{c.question.value}</h5>
                    <p>Ответ пользователя:</p>
                    {c.question.answerType === 1
                        ? <div>
                            <input type="text" className="form-control" placeholder="Пользователь не ответил на этот вопрос."
                                readOnly value={c.answer.value} />
                        </div>
                        : <div className="form-group form-check">
                            {c.question.answers.map(ans =>
                                <div key={ans.answerId}>
                                    <input type="radio" className="form-check-input d-inline"
                                        readOnly
                                        checked={ans.number === parseInt(c.answer.value)} />
                                    <label className="form-check-label">{ans.value}</label>
                                    {ans.number === c.userAnswer
                                        ? <p>(ваш ответ)</p>
                                        : null
                                    }
                                </div>)}
                            {!c.answer.value
                                ? <p>(Пользователь не ответил на этот вопрос)</p>
                                : null}
                        </div>
                    }
                    <div className="btn-toolbar">
                        <div className="btn-group">
                            <button className={!c.answer.isCorrect
                                ? "btn btn-danger"
                                : "btn btn-outline-danger"}
                                disabled={!c.answer.isCorrect}
                                onClick={e => this.handleChange(e, false, c.question.number)}>
                                Неверно
                            </button>
                            <button className={c.answer.isCorrect
                                ? "btn btn-success"
                                : "btn btn-outline-success"}
                                disabled={c.answer.isCorrect}
                                onClick={e=> this.handleChange(e, true, c.question.number)}>
                                Верно
                            </button>
                        </div>
                    </div>
                </div>
            })}
            <button className="btn btn-outline-primary"
                disabled={isSaved}
                onClick={e => this.handleSave(e)}>Сохранить</button>
        </div>
    }

    handleBack = event => {
        event.preventDefault();
        const { isSaved } = this.state;
        if (!isSaved && !confirm("У вас есть несохранённые изменения. Они могут пропасть. Вы уверены?")) return;
        this.props.onBrowsingEnd();
    }

    handleSave = async event => {
        event.preventDefault();
        const { testId, attemptId } = this.props.info;
        const { correctAQNumbers, incorrectAQNumbers } = this.state;
        const formData = new FormData();

        formData.append('model.testId', testId);
        formData.append('model.attemptId', attemptId);

        correctAQNumbers.forEach((item, index) => {
            formData.append(`model.CorrectAQNumbers[${index-1}]`, item);
        });

        incorrectAQNumbers.forEach((item, index) => {
            formData.append(`model.IncorrectAQNumbers[${index-1}]`, item);
        });
        await fetch('/api/profile/check-test', {
            method: 'POST',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                this.setState({ isSaved: true });
            } else alert(`Произошла непредвиденная ошибка. Попробуйте снова. ${response.status}`);
        })
    }

    handleChange = (event, isCorrect, questionNumber) => {
        event.preventDefault();
        const { correctAQNumbers, incorrectAQNumbers } = this.state;

        if (isCorrect) {
            correctAQNumbers.add(questionNumber);
            incorrectAQNumbers.delete(questionNumber);
        } else {
            incorrectAQNumbers.add(questionNumber);
            correctAQNumbers.delete(questionNumber);
        }
        this.setState({ isSaved: false, correctAQNumbers: correctAQNumbers, incorrectAQNumbers: incorrectAQNumbers });
    }
}