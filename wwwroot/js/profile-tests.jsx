class UserTestsList extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            isLoading: true,
            isEmpty: false,
            tests: [],
            browsingInfo: {
                isBrowsing: false,
                browsedTest: {}
            }
        };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const { isLoading, isEmpty, tests, browsingInfo } = this.state;
        return (<div>
            {isLoading
                ? <h4>Пройденные вами тесты загружаются...</h4>
                : isEmpty
                    ? <div>
                        <h3>Вы ещё не проходили ни одного теста.</h3>
                        <a className="btn btn-outline-primary" href="/tests/index">Выбрать и пройти тест</a>
                    </div>
                    : browsingInfo.isBrowsing
                        ? <DetailedPassedTest
                            info={browsingInfo.browsedTest}
                            onBrowsingEnd={this.handleBrowsingEnd}
                        />
                        : tests.map(test => {
                            return <div key={test.testId}>
                                <hr />
                                <PassedTestSummary key={test.testId}
                                    info={test}
                                    onTestBrowsing={this.handleTestBrowsing}
                                    onRateChanged={this.handleRateChange}
                                />
                                <hr />
                            </div>;
                    })
            }
        </div>);
    }

    populateData = async () => {
        await fetch('/api/profile/passed-tests-info').then(async response => {
            if (response.status === 204) {
                this.setState({ isLoading: false, isEmpty: true });
            }
            else if (response.status === 200) {
                const object = await response.json();
                const result = object.result;
                this.setState({ isLoading: false, tests: result });
            }
            else alert(`Произошла ошибка при попытке получить данные с сервера. ${response.status}`);
        });
    }

    handleTestBrowsing = async (event, testId) => {
        event.preventDefault();

        await fetch(`/api/profile/detailed-test?testId=${testId}`)
            .then(async response => {
                if (response.status === 200) {
                    const object = await response.json();
                    const result = object.result;
                    this.setState({
                        browsingInfo: {
                            isBrowsing: true, browsedTest: result
                        }
                    });
                } else alert(`Произошла ошибка при попытке получить данные с сервера. ${response.status}`);
            });
    }

    handleBrowsingEnd = async event => {
        event.preventDefault();

        this.setState({
            browsingInfo: {
                isBrowsing: false,
                browsedTest: {}
            }
        });
    }

    handleRateChange = (testId, newRate) => {
        const { tests } = this.state;
        const test = tests.find(t => t.testId === testId);
        test.userRate = newRate;
        this.setState({ tests: tests });
    }
}

class PassedTestSummary extends React.Component {
    constructor(props) {
        super(props);

    }

    render() {
        const { testId, testName, lastTimePassed, attemptsUsed, userRate } = this.props.info;
        const rates = ["ужасен", "плох", "норм", "хорош", "прекрасен"],
            colors = ["danger", "warning", "primary", "info", "success"];
        return (<div>
            <h3>Тест {testName}</h3>
            <p>Попыток: {attemptsUsed}</p>
            <p>Время последней попытки: {lastTimePassed}</p>
            <button className="btn btn-outline-success"
                onClick={e => this.props.onTestBrowsing(e, testId)}>Просмотреть своё последнее решение</button>
            <h4>Оценить тест</h4>
            <div className="btn-toolbar">
                <div className="btn-group">
                    <button className="btn btn-outline-danger"
                        disabled={userRate === 1}
                        onClick={e => this.handleRate(e, 1)}>Ужасен</button>
                    <button className="btn btn-outline-warning"
                        disabled={userRate === 2}
                        onClick={e => this.handleRate(e, 2)}>Плох</button>
                    <button className="btn btn-outline-primary"
                        disabled={userRate === 3}
                        onClick={e => this.handleRate(e, 3)}>Норм</button>
                    <button className="btn btn-outline-info"
                        disabled={userRate === 4}
                        onClick={e => this.handleRate(e, 4)}>Хорош</button>
                    <button className="btn btn-outline-success"
                        disabled={userRate === 5}
                        onClick={e => this.handleRate(e, 5)}>Прекрасен</button>
                </div>
            </div>
            {userRate === -1
                ? null
                : <div className="d-flex">
                    <h6>Ваша оценка: </h6>
                    <h6 className={`text-${colors[userRate - 1]}`}>{rates[userRate - 1]}</h6>
                </div>
            }
        </div>);
    }
    handleRate = async (event, rate) => {
        event.preventDefault();

        const { testId } = this.props.info;

        const formData = new FormData();
        formData.append('testId', testId);
        formData.append('rate', rate);

        await fetch('/api/tests/rate-test', {
            method: 'POST',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                this.props.onRateChanged(testId, rate);
            } else alert(`Произошла ошибка при попытке оценить тест. Попробуйте снова. ${response.status}`);
        });
    }
}

class DetailedPassedTest extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const { testName,
            questions,
            userAnswers,
            correctAnswersCount,
            areAnswersManuallyChecked,
            isChecked } = this.props.info;

        const showChecks = isChecked
            ? true
            : areAnswersManuallyChecked
                ? false
                : true;

        const combined = [];
        let i = 0;
        for (const question of questions) {
            i++;
            const userAnswer = userAnswers.find(a => a.questionNumber === question.number);

            if (!!userAnswer) {

                combined.push({
                    id: i,
                    question: question,
                    answer: {
                        value: !!userAnswer.value && userAnswer.value !== 'null'
                            ? userAnswer.value
                            : "",
                        questionNumber: question.number,
                        isCorrect: userAnswer.isCorrect
                    }
                });
            } else {

                combined.push({
                    id: i,
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
            <button className="btn btn-outline-primary" onClick={this.props.onBrowsingEnd}>К тестам</button>
            <h2 className="display-2 text-center">Тест {testName}</h2>
            {areAnswersManuallyChecked
                ? isChecked
                    ? <p className="text-success m-0">Работа проверена.</p>
                    : <p className="text-danger m-0">Создатель теста ещё не проверил вашу работу.</p>
                : null
            }
            {combined.map(c => {
                return <div key={c.id}>
                    <hr />
                    <h4>Вопрос {c.question.number}</h4>
                    <h5 className="display-5">{c.question.value}</h5>
                    <p>Ваш ответ:</p>
                    {c.question.answerType === 1
                        ? <div>
                            <input type="text" className="form-control" placeholder="Вы не ответили на этот вопрос."
                                readOnly value={c.answer.value} />
                            {showChecks
                                ? !c.answer.value
                                    ? null
                                    : c.answer.isCorrect
                                        ? <b className="text-success">Это верный ответ.</b>
                                        : <p className="text-danger">
                                            Это неверный ответ.
                                            {areAnswersManuallyChecked
                                                ? null
                                                : <b className="text-dark">Верный ответ: {c.question.answer}</b>
                                            }
                                            </p>
                                : null
                            }
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
                                ? <p>(Вы не ответили на этот вопрос)</p>
                                : null}
                        </div>
                    }
                </div>
            })}
            {showChecks
                ? <p>Правильных ответов: <b>{correctAnswersCount + "/" + questions.length}</b></p>
                : <p>Работа ещё не проверена.</p>
            }
        </div>;
    }
}

ReactDOM.render(<UserTestsList />, document.getElementById("root"));