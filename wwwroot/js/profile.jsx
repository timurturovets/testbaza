class ManageAccountForm extends React.Component {
    constructor(props) {
        super(props);

        this.state = { isLoading: true, user: null };

        this.populateData = this.populateData.bind(this);
        this.renderForm = this.renderForm.bind(this);
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const isLoading = this.state.isLoading;
        return (
        <div className="text-center">
            { isLoading
                ? <h3>Загрузка...</h3>
                : this.renderForm()
            }
        </div>
        );
    }

    renderForm() {
        const user = this.state.user;
        return (<div>
            <form>
                <UserNameInput defaultUsername={user.UserName} handleChange={this.onNameChanged} />
                <EmailInput defaultEmail={user.Email} handleChange={this.onEmailChanged} />
                <PasswordInput defaultPassword={user.Password} handleChange={this.onPasswordChanged} />
            </form>
        </div>);
    }

    async populateData() {
        await fetch('/profile/user-info').then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                this.setState({ isLoading: false, user: result });
            } else {
                window.location.replace('/');
            }
        });
    }

    onNameChanged(newUserName) {
        if (newUserName === "" || newUserName === null) return { isSuccess: false, error: 'Вы не ввели имя' };
        if (newUserName.length < 4) return { isSuccess: false, error: 'Имя должно быть длиной не менее 3 символов' };
        if (newUserName.length > 20) return { isSuccess: false, error: 'Имя должно быть длиной не более 20 символов' };

        const user = this.state.user;
        user.UserName = newUserName;

        this.setState({ user: user });
    }

    onEmailChanged(newEmail) {
        const emailRegex = /^([0-9]|[a-zA-Z])+@.+\..+$/ig;
        if (!emailRegex.test(newEmail)) return { isSuccess: false, error: 'Вы ввели некорректный адрес эл.почты' };

        const user = this.state.user;
        user.Email = newEmail;

        this.setState({ user: user });
    }
}

class UserNameInput extends React.Component {
    constructor(props) {
        super(props);

        this.state = { isChanged: false, isSuccess: false, error: null };

        this.onBlur = this.onBlur.bind(this);
    }

    render() {
        return
            <div className="form-group">
                <label>Имя пользователя</label>
                <input type="text" className="form-control" name="username"
                    defaultValue={this.props.defaultUsername} onBlur={e => this.onBlur(e)} />
        </div>
    }

    onBlur(event) {
        const newUsername = event.target.value;
        const { isSuccess, error } = this.props.handleChange(newUsername);
        this.setState({ isChanged: true, isSuccess: isSuccess, error: error });
    }
}

class EmailInput extends React.Component {
    constructor(props) {
        super(props);

        this.state = { isChanged: false, isSuccess: false, error: null };

        this.onBlur = this.onBlur.bind(this);
    }

    render() {
        return (
            <div className="form-group">
                <input type="text" className="form-control" name="email"
                    defaultValue={this.props.defaultUsername} onBlur={e => this.onBlur(e)} />
            </div>
        );
    }

    onBlur(event) {
        const newEmail = event.target.value;
        const { isSuccess, error } = this.props.handleChange(newEmail);
        this.setState({ isChanged: true, isSuccess: isSuccess, error: error });
    }
}

class PasswordInput extends React.Component {
    //TODO
}