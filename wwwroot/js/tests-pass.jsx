class Test extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            isLoading: true,
            passingInfo: {
                isStarted: false,
                timeStarted: null,
                timeEnd: null,
                timeLeft: null,
                isTimeOut: false,
                interval: null,
                isCheckingAnswers: false,
                attemptsLeft: 0,
                isContinuing: false
            },
            test: null,
            answers: [],
            currentQuestion: 0,
            exceededAttempts: false
        };
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const content = this.state.isLoading
            ? (<div>
                <h2>Прохождение теста</h2>
                <h4>Загрузка теста...</h4>
            </div >)
            : this.state.exceededAttempts
                ? <div>
                    <h2 className="display-2">У вас не осталось попыток для прохождения этого теста.</h2>
                    <a className="btn btn-outline-primary" href="/profile/user-tests">Просмотреть своё решение</a>
                </div>
                : this.renderTest();

        return content;
    }

    populateData = async () => {
        const testId = this.props.testId;
        await fetch(`/api/pass/info?id=${testId}`).then(async response => {
            console.log(response.status);
            if (response.status === 201) {
                //Нет текущей попытки
                const object = await response.json();
                console.log(`object`);
                console.log(object);
                const test = object.result.test;
                const answers = test.questions.map(q => new UserAnswer(q.number, null));

                this.setState({
                    isLoading: false,
                    test: test,
                    answers: answers,
                    passingInfo: {
                        ...this.state.passingInfo,
                        attemptsLeft: object.result.attemptsLeft
                    }
                });

            } else if (response.status === 200) {
                //Есть текущая попытка
                const object = await response.json();
                console.log(`object`);
                console.log(object);
                const result = object.result;
                const timeLeft = result.timeLeft * 1000,
                    test = result.test,
                    answers = result.userAnswers;
                const userAnswers = answers.map(answer => new UserAnswer(answer.questionNumber, answer.value));

                for (const question of test.questions) {
                    const number = question.number;
                    let answered = userAnswers.some(a => a.questionNumber === number);
                    if (!answered) userAnswers.push(new UserAnswer(number, null));
                }

                this.setState({
                    isLoading: false,
                    test: test,
                    answers: userAnswers,
                    passingInfo: {
                        ...this.state.passingInfo,
                        isStarted: true,
                        timeLeft: timeLeft,
                        isContinuing: true
                    }
                });
            } else if (response.status === 409) {
                this.setState({ isLoading: false, exceededAttempts: true });
            } else alert(`status: ${response.status}`);
        });
    }

    renderTest = () => {
        const { _, passingInfo, test } = this.state;
        const timeInfo = test.timeInfo;

        const timeLimitString = getCompleteTimeString(timeInfo.hours * 3600 + timeInfo.minutes * 60 + timeInfo.seconds);
        return (<div>
            <h3 className="text-center display-3">Тест {test.testName}</h3>

            {passingInfo.isStarted
                ? passingInfo.isTimeOut
                    ? <h1 className="text-center">Время вышло!</h1>
                    : this.renderActiveTest()
                : <div className="text-center">
                    {timeInfo.isTimeLimited
                        ? <h5>Ограничение по времени: {timeLimitString}</h5>
                        : <h5>Ограничения по времени нет</h5>
                    }
                    {test.areAttemptsLimited
                        ? <h5>Осталось попыток: {passingInfo.attemptsLeft}</h5>
                        : <h5>Количество попыток неограничено</h5>
                    }
                    </div>
            }
   
            <div className="text-center">
            {passingInfo.isStarted
                ? <button className="btn btn-outline-success" onClick={this.handleSubmit}>Отправить решение</button>
                : <button className="btn btn-outline-primary" onClick={this.handleStart}>Начать</button>
                }
            </div>

        </div>);
    }

    renderActiveTest = () => {
        const { _, passingInfo, test, currentQuestion, answers } = this.state;
        const question = test.questions[currentQuestion];
        const answer = answers.find(a => a.questionNumber === question.number);
        return <div>
            {test.timeInfo.isTimeLimited
                ? <Timer
                    onTimeOut={this.handleTimeout} 
                    timeInfo={test.timeInfo}
                    isContinuing={passingInfo.isContinuing}
                    timeLeft={passingInfo.isContinuing ? passingInfo.timeLeft : null}
                />
                : null}
            
            <Question key={question.number}
                info={question}
                onAnswerChanged={this.handleAnswerChange}
                onAnswerSaved={this.handleAnswerSave}
                userAnswer={answer.value}
                isBrowsing={false} />
            {this.renderQuestionsNavigation()}
        </div>
    }

    renderQuestionsNavigation = () => {
        const test = this.state.test,
            currentQuestion = this.state.currentQuestion;

        return <div className="btn-toolbar">
            <div className="btn-group">
                <button className="btn btn-outline-primary" onClick={this.handlePrevQuestion}
                    disabled={currentQuestion === 0}>Предыдущий вопрос</button>

                <button className="btn btn-outline-primary"
                    onClick={this.handleNextQuestion}
                    disabled={currentQuestion >= test.questions.length - 1}>Следующий вопрос</button>
            </div>
        </div>
    }

    handleAnswerChange = (questionNumber, value) => {
        const { answers } = this.state;
        const answer = answers.find(a => a.questionNumber === questionNumber);
        answer.value = value;
        this.setState({ answers: answers });
    }

    handleAnswerSave = async (questionNumber, value) => {
        const testId = this.props.testId;

        const formData = new FormData();
        formData.append('testId', testId);
        formData.append('questionNumber', questionNumber);
        formData.append('value', value);

        this.fetchAndSaveAnswer(formData).then(async response => {
            if (response.status === 200) {
                const answers = this.state.answers;
                for (const answer of answers) {
                    if (answer.questionNumber === questionNumber) {
                        answer.value = value;
                        this.setState({ answers: answers });
                        break;
                    }
                }
            } else alert(`status ${response.status}`);
        });

    }

    fetchAndSaveAnswer = async (formData = new FormData()) => {
        return await fetch('/api/pass/save-answer', {
            method: 'POST',
            body: formData
        })
    }

    handleNextQuestion = async event => {
        event.preventDefault();

        let { test, answers, currentQuestion } = this.state;
        console.log(test);
        const question = test.questions[currentQuestion];
        const userAnswer = answers.find(a => a.questionNumber === question.number);
        console.log(`user answer is` + userAnswer);
        if (test.questions.length - 1 !== currentQuestion) {
            currentQuestion++;
            this.setState({ currentQuestion: currentQuestion });

            const testId = this.props.testId;

            const formData = new FormData();
            formData.append('testId', testId);
            formData.append('questionNumber', question.number);
            formData.append('value', userAnswer.value);
            await this.fetchAndSaveAnswer(formData).then(async response => {
                if (response.status === 200) {
                    const answers = this.state.answers;
                    for (const answer of answers) {
                        if (answer.questionNumber === question.number) {
                            answer.value = userAnswer.value;
                            this.setState({ answers: answers });
                            break;
                        }
                    }
                } else alert(`status ${response.status}`);
            });
        }
    }

    handlePrevQuestion = async event => {
        event.preventDefault();

        let { test, answers, currentQuestion } = this.state;
        const question = test.questions[currentQuestion];
        const userAnswer = answers.find(a => a.questionNumber === question.number);
        if (currentQuestion > 0) {
            currentQuestion--;
            this.setState({ currentQuestion: currentQuestion });

            const testId = this.props.testId;

            const formData = new FormData();
            formData.append('testId', testId);
            formData.append('questionNumber', question.number);
            formData.append('value', userAnswer.value);
            await this.fetchAndSaveAnswer(formData).then(async response => {
                if (response.status === 200) {
                    const answers = this.state.answers;
                    for (const answer of answers) {
                        if (answer.questionNumber === question.number) {
                            answers[answers.indexOf(answer)].value = userAnswer.value;
                            this.setState({ answers: answers });
                            break;
                        }
                    }
                } else alert(`status ${response.status}`);
            });
        }
    }

    handleStart = async event => {
        event.preventDefault();

        await fetch(`/api/pass/start-passing?testId=${this.props.testId}`).then(response => {
            if (response.status === 409) {
                this.setState({ exceededAttempts: true });
            } else if (response.status === 200) {
                const passingInfo = this.state.passingInfo;
                this.setState({ passingInfo: { ...passingInfo, isStarted: true } });
            }
        })
    }

    handleSubmit = async event => {
        event.preventDefault();
        const { answers } = this.state;

        const testId = this.props.testId;

        for (const answer of answers) {
            const formData = new FormData();
            formData.append('testId', testId);
            formData.append('questionNumber', answer.questionNumber);
            formData.append('value', answer.value);

            await this.fetchAndSaveAnswer(formData).then(async response => {
                if(response.status !== 200 )alert(`Произошла непредвиденная ошибка. Попробуйте снова. ${response.status}`);
            })
        }
        
        await fetch(`/api/pass/end-passing?testId=${testId}`)
            .then(async response => {
                if (response.status === 200) {
                    window.location.href = "/profile/user-tests"
                } else alert(`Произошла ошибка при попытке сохранить прохождение теста. ${response.status}`);
            });
    }

    handleTimeout = () => {
        const passingInfo = this.state.passingInfo;
        alert('Время вышло!');
        this.setState({ passingInfo: { ...passingInfo, isTimeOut: true } });
    }
}

class Timer extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            interval: null,
            timeLeft: props.isContinuing
                ? props.timeLeft
                : 0
        };
    }

    componentDidMount() {
        let { interval, timeLeft } = this.state;
        const { onTimeOut, timeInfo, isContinuing } = this.props;
        const now = new Date().getTime();
        const end = now
            + timeInfo.hours * 3600 * 1000
            + timeInfo.minutes * 60 * 1000 +
            timeInfo.seconds * 1000;
        
        if (!isContinuing) timeLeft = end - now;

        interval = setInterval(() => {
            timeLeft -= 1000;
            if (timeLeft <= 0) {
                onTimeOut();
                clearInterval(interval);
                return;
            }
            this.setState({ timeLeft: timeLeft });
        }, 1000);

        this.setState({ interval: interval });
    }

    componentWillUnmount() {
        clearInterval(this.state.interval);
    }

    render() {
        const timeLeft = this.state.timeLeft;
        const allSeconds = timeLeft / 1000;

        const hours = Math.floor(allSeconds / 3600);
        const hh = hours === 0
            ? "00"
            : Math.floor(hours / 10) === 0
                ? `0${hours}`
                : hours;

        const minutes = Math.floor((allSeconds - hours * 3600) / 60);
        const mm = minutes === 0
            ? "00"
            : Math.floor(minutes / 10) === 0
                ? `0${minutes}`
                : minutes;

        const seconds = allSeconds - hours * 3600 - minutes * 60;
        const ss = seconds === 0
            ? "00" : Math.floor(seconds / 10) === 0
                ? `0${seconds}`
                : seconds;

        const timeLeftString = `${hh}:${mm}:${ss}`;
        return (<div>
            {allSeconds === 0
                ? null
                : allSeconds < 60
                    ? <div className="text-danger">
                        <b>Осталось меньше минуты!</b>
                        <p>{timeLeftString}</p>
                    </div>
                    : <p>Осталось: {timeLeftString}</p>
            }
        </div>);
    }
}

class Question extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            isSaved: true,
            isHintActive: false,
            userAnswer: this.props.userAnswer
        };
    }

    render() {
        const { info, isBrowsing } = this.props,
            { isSaved, isHintActive, userAnswer } = this.state;
        console.log(`User answer when rendering question: ${userAnswer}`);
        return (<div>
            <h3>Вопрос {info.number}</h3>
            <h4 className="display-3">{info.value}</h4>
            {info.answerType === 1
                ? <div className="form-group">
                    <label>Ваш ответ:</label>
                    <input key={`q-${info.number}`} type="text" className="form-control" placeholder="Ваш ответ"
                        onChange={this.onAnswerChanged} defaultValue={!!userAnswer ? userAnswer : ""} readOnly={isBrowsing} />
                </div>
                : <div>
                    {info.answers.map(answer =>
                        <div key={answer.number} className="form-check">
                            <input key={answer.number} className="form-check-input" type="radio" name={`radio-answer`}
                                defaultValue={`${answer.number}`}
                                checked={parseInt(userAnswer) === answer.number}
                                onChange={this.onAnswerChanged}
                            />
                            <label className="form-check-label">{answer.value}</label>
                        </div>
                    )
                    }
                </div>
            }
            {info.hintEnabled
                ? <button className={isHintActive ? "btn btn-outline-secondary" : "btn btn-outline-success"}
                    onClick={e => this.setState({ isHintActive: !isHintActive })}>
                    {isHintActive
                        ? info.hint
                        : "Показать подсказку"
                    }
                    </button>
                : null}
            <button className="btn btn-outline-success" onClick={this.onAnswerSaved}
                disabled={isSaved}>Сохранить ответ</button>
        </div>);
    }
    
    onAnswerChanged = event => {
        let newValue = event.target.value;
        if (!isNaN(parseInt(newValue))) newValue = parseInt(newValue);
        this.setState({ isSaved: false, userAnswer: newValue });
        this.props.onAnswerChanged(this.props.info.number, newValue);
    }

    onAnswerSaved = () => {
        const questionNumber = this.props.info.number;
        const answer = this.state.userAnswer;
        this.props.onAnswerSaved(questionNumber, answer);
        this.setState({ isSaved: true });
    }
}

class UserAnswer {
    constructor(questionNumber, value) {
        this.questionNumber = questionNumber;
        this.value = value;
    }
}

function getCompleteTimeString(allSeconds = 0) {
    return `${getTimeString(allSeconds, "час")} ${getTimeString(allSeconds, "минут")} ${getTimeString(allSeconds, "секунд")}`;
}

function getTimeString (allSeconds = 0, starting = "") {
    starting = starting.toLowerCase();
    const hours = Math.floor(allSeconds / 3600);
    const minutes = Math.floor((allSeconds - hours * 3600) / 60);
    const seconds = allSeconds - hours * 3600 - minutes * 60;
    if (starting === 'час') {

        if (hours == 0) return "";
        if (Math.floor(hours / 10) == 1) return `${hours} часов`;
        if (hours % 10 == 1) return `${hours} час`
        if (hours % 10 > 1 && hours % 10 < 5) return `${hours} часа`;
        return `${hours} часов`;

    } else if (starting === 'минут') {

        if (minutes == 0) return "";
        if (Math.floor(minutes / 10) == 1) return `${minutes} минут`;
        if (minutes % 10 == 1) return `${minutes} минута`
        if (minutes % 10 > 1 && minutes % 10 < 5) return `${minutes} минуты`;
        return `${minutes} минут`;

    } else if (starting === 'секунд') {

        if (seconds == 0) return "";
        if (Math.floor(seconds / 10) == 1) return `${seconds} секунд`;
        if (seconds % 10 == 1) return `${seconds} секунда`
        if (seconds % 10 > 1 && seconds % 10 < 5) return `${seconds} секунды`;
        return `${seconds} секунд`;

    } else throw new Error("Некорректные значения строки. Разрешены только 'час', 'минут', 'секунд'");
};
