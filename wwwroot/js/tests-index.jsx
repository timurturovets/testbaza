class TestSummary extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return (<div>
            <h2 className="display-2">Тест "{this.props.name}"</h2>
            <p>Создан {this.props.timeCreated}</p>
            <p>Автор: {this.props.authorName}</p>
            <p>Вопросов: {this.props.questionsCount}</p>
            <a className="btn btn-outline-success" href={`/tests/pass?id=${this.props.id}`}>Пройти тест</a>
        </div>);
    }
}

class TestsList extends React.Component {
    constructor(props) {
        super(props);
        this.populateTests = this.populateTests.bind(this);
        this.renderTests = this.renderTests.bind(this);
        this.state = {isLoading: true, tests: []}
    }

    componentDidMount() {
        this.populateTests();
    }

    render() {
        const content = this.state.isLoading
            ? <h1 className="display-4">Загрузка...</h1>
            : this.renderTests();

        return content;
    }

    async populateTests() {
        const response = await fetch('/api/tests/all');
        if (response.status == 200) {
            await response.json().then(result => {
                console.log(result);
                this.setState({ isLoading: false, tests: result.tests });
            })
        } else if (response.status === 204) this.setState({ isLoading: false, tests: [] });
        else alert("Произошла непредвиденная ошибка. Попробуйте перезагрузить страницу");
    }

    renderTests() {
        console.log(this.state);
        return this.state.tests.length > 0
            ? (<div>
                {
                    this.state.tests.map(test => {
                        return(
                        <div key={test.testName}>
                            <TestSummary id={test.id}
                                name={test.testName}
                                authorName={test.authorName}
                                questionsCount={test.questionsCount}
                                timeCreated={test.timeCreated}
                            />
                            <hr />
                        </div>)
                    })
                }
            </div>)

        : (<p>Пока-что ещё не создано ни одного теста.
                <a className="btn btn-outline-success m-1" href="/tests/create">Создайте первый! :)</a>
        </p>)
    }
}

ReactDOM.render(
    <TestsList />,
    document.getElementById('root')
)