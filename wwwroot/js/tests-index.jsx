class TestSummary extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return (<Fragment>
            <h1 className="display-1">Тест "{this.props.name}"</h1>
            <p>Создан {this.props.whenCreated}</p>
            <p>Автор: {this.props.authorName}</p>
            <p>Вопросов: {this.props.questionsCount}</p>
            <a className="btn btn-outline-success" href={`/tests/pass?id=${this.props.id}`}>Пройти тест</a>
        </Fragment>);
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
        const response = await fetch('/tests/all');
        if (response.status == 200) {
            await response.json().then(result => {
                this.setState({ isLoading: false, tests: result });
            })
            .catch(err => {
                alert(`Ошибка при попытке получить данные с сервера: ${err}`);
            });
        } else alert(`При попытке получить данные с сервера был получен статус ${response.status}`)
    }

    renderTests() {
        return this.state.tests.length > 0 ?
            <Fragment>
                {
                    this.state.tests.map(test => {
                        <div key={test.id}>
                            <TestSummary id={test.id}
                                name={test.name}
                                authorName={test.authorName}
                                questionsCount={test.questions.length}
                                whenCreated={test.timeCreated}
                            />
                            <hr />
                        </div>
                    })
                }
            </Fragment>

        : <Fragment>Пока-что ещё не создано ни одного теста.
                <a className="btn btn-outline-success" href="/tests/create">Создайте первый! :)</a>
        </Fragment>
    }
}

ReactDOM.render(
    <TestsList />,
    document.getElementById('root')
)