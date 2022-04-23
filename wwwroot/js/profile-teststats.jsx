class TestStat extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            isLoading: true,
            isBrowsingStat: false,
            browsedStat: {},
            stats: [],
            isEmpty: false
        }
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const { isLoading, isBrowsingStat, browsedStat } = this.state;
        const content = isLoading
            ? <h3>Загрузка информации о прохождении...</h3>
            : isBrowsingStat
                ? <UserStat info={browsedStat} onBrowsingEnd={this.handleBrowsingEnd}/>
                : this.renderStats();
        return content;
    }

    populateData = async () => {
        await fetch(`/api/profile/get-stats?id=${this.props.testId}`).then(async response => {
            if (response.status === 204) {
                this.setState({ isLoading: false, isEmpty: true });
            } else if (response.status === 200) {
                const object = await response.json();
                const result = object.result;
                this.setState({ isLoading: false, stats: result, isEmpty: false });
            } else alert(`Произошла непредвиденная ошибка. Попробуйте снова. ${response.status}`);
        })
    }

    handleStatBrowsing = async (event, attemptId) => {
        event.preventDefault();

        const testId = this.props.testId;
        await fetch(`/api/profile/get-stat?testId=${testId}&attemptId=${attemptId}`).then(async response => {
            if (response.status === 200) {
                const object = await response.json();
                const result = object.result;
                this.setState({ isBrowsingStat: true, browsedStat: result });
            } else alert(`Произошла непредвиденная ошибка. Попробуйте снова. ${response.status}`);
        });
    }

    handleBrowsingEnd = event => {
        event.preventDefault();

        this.setState({ isBrowsingStat: false, browsedStat: {} });
    }
    renderStats = () => {
        const { stats, isEmpty } = this.state;
        return <div>
            {isEmpty
                ? <h4>Этот тест ещё никто не проходил :(</h4>
                : <div>
                    {stats.map(stat => <UserStatSummary key={stat.attemptId}
                        info={stat}
                        onStatBrowsing={this.handleStatBrowsing} />)}
                </div>
            }
        </div>
    }
}

class UserStatSummary extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const { attemptId, userName, lastTimePassed } = this.props.info;
        return <div>
            <h3>Пользователь {userName}</h3>
            <p>Последний раз проходил этот тест: {lastTimePassed}</p>
            <button className="btn btn-outline-success"
                onClick={e=>this.props.onStatBrowsing(e, attemptId)}>Просмотреть решение</button>
        </div>
    }
}

class UserStat extends React.Component {
    constructor(props) {
        super(props);

    }

    render() {
        const { questions, userAnswers, correctAnswersCount } = this.props.info;

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
            <button className="btn btn-outline-primary" onClick={this.props.onBrowsingEnd}>Назад</button>
            {combined.map(c => {
                return <div key={c.id}>
                    <hr />
                    <h4>Вопрос {c.question.number}</h4>
                    <h5 className="display-5">{c.question.value}</h5>
                    <p className="m-0">Ответ пользователя:</p>
                    {c.question.answerType === 1
                        ? <div>
                            <input type="text" className="form-control" placeholder="Пользователь не ответил на этот вопрос."
                                readOnly value={c.answer.value} />
                            {!c.answer.value
                                ? null
                                : c.answer.isCorrect
                                    ? <b className="text-success">Это верный ответ.</b>
                                    : <p className="text-danger">
                                            Это неверный ответ.
                                            <b className="text-dark">Верный ответ: {c.question.answer}</b>
                                        </p>
                            }
                        </div>
                        : <div className="form-group form-check">
                            {c.question.answers.map(ans =>
                                <div key={ans.questionNumber}>
                                    <input type="radio" className="form-check-input d-inline"
                                        readOnly
                                        checked={ans.number === parseInt(c.answer.value)} />
                                    <label className="form-check-label">{ans.value}</label>
                                </div>)}
                            {!c.answer.value
                                ? <p>(Пользователь не ответил на этот вопрос)</p>
                                : c.question.correctAnswerNumber === parseInt(c.answer.value)
                                    ? <b className="text-success">Это верный ответ.</b>
                                    : <p className="text-danger">
                                        Это неверный ответ. <br />
                                        <b className="text-dark">Верный ответ: 
                                            {c.question.answers.find(a => a.number === c.question.correctAnswerNumber)
                                                .value}
                                        </b>
                                    </p>}
                        </div>
                    }
                </div>
            })}
            <p>Правильных ответов: <b>{correctAnswersCount + "/" + questions.length}</b></p>
        </div>
    }
}