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
                ? <h4>Ваши тесты загружаются...</h4>
                : isEmpty
                    ? <div>
                        <h3>Вы ещё не проходили ни одного теста.</h3>
                        <a className="btn btn-outline-primary" href="/tests/all">Выбрать и пройти тест</a>
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
                                    onTestBrowsing={this.handleTestBrowsing} />
                                <hr />
                            </div>;
                    })
            }
        </div>);
    }

    populateData = async () => {
        await fetch('/api/profile/passed-tests-info').then(async response => {
            console.log(response.status);
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
}

class PassedTestSummary extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const { testId, testName, lastTimePassed, attemptsUsed } = this.props.info;
        return (<div>
            <h3>Тест {testName}</h3>
            <p>Попыток: {attemptsUsed}</p>
            <p>Время последней попытки: {lastTimePassed}</p>
            <button className="btn btn-outline-success"
                onClick={e => this.props.onTestBrowsing(e, testId)}>Просмотреть своё последнее решение</button>
        </div>);
    }
}

class DetailedPassedTest extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const test = this.props.info;
        console.log(test);
        const questions = test.questions,
            answers = test.userAnswers;

        const combined = [];
        let i = 0;
        for (const question of questions) {
            i++;
            const userAnswer = answers.find(a => a.questionNumber === question.number);

            if (!!userAnswer) {

                combined.push({
                    id: i,
                    question: question,
                    answer: userAnswer
                });

            } else {

                combined.push({
                    id: i,
                    question: question,
                    answer: {
                        value: "",
                        questionNumber: question.number
                    }
                });
            }
        }

        return <div>
            <button className="btn btn-outline-primary" onClick={this.props.onBrowsingEnd}>К тестам</button>
            <h2 className="display-2 text-center">Тест {test.testName}</h2>
            {console.log('combined: ')}
            {console.log(combined)}
            {combined.map(c => {
                return <div key={c.id}>
                    <h4>Вопрос {c.question.number}</h4>
                    <h5>{c.question.value}</h5>
                    <p>Ваш ответ:</p>
                    {c.question.answerType === 1
                        ? <div>
                            <input type="text" className="form-control" placeholder="Вы не ответили на этот вопрос."
                                readOnly value={c.answer.value} />
                        </div>
                        : <div className="form-group form-check">
                            {c.question.answers.map(ans =>
                                <div key={ans.questionNumber}>
                                    <input type="radio" className="form-check-input d-inline"
                                        readOnly
                                        checked={ans.number === c.answer.value} />
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
        </div>;
    }
    handleNext
}

ReactDOM.render(<UserTestsList />, document.getElementById("root"));