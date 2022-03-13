class ManageAccountForm extends React.Component {
    constructor(props) {
        super(props);

        this.populateData = this.populateData.bind(this);
        this.renderForm = this.renderForm.bind(this);
        this.onNameChanged = this.onNameChanged.bind(this);
        this.onEmailChanged = this.onEmailChanged.bind(this);
        this.onPasswordChanged = this.onPasswordChanged.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);

        this.state = { isLoading: true, isSaved: true, user: null };
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
                {this.state.isSaved
                    ? <button className="btn btn-outline-primary"
                        onClick={e => this.handleSubmit(e)} disabled>Сохранить изменения</button>
                    : <button className="btn btn-outline-primary"
                        onClick={e => this.handleSubmit(e)}>Сохранить изменения</button>
                }
            </form>
        </div>);
    }

    async populateData() {
        await fetch('/profile/user-info').then(async response => {
            if (response.status === 200) {
                const result = await response.json();
                result.oldPassword = "";
                this.setState({ isLoading: false, user: result });
            } else {
                window.location.replace('/');
            }
        });
    }

    async handleSubmit(e) {
        e.preventDefault();
        const user = this.state.user;

        const formData = new FormData();
        formData.append('username', user.username);
        formData.append('email', user.email);
        formData.append('newpassword', user.password);
        formData.append('oldpassword', user.oldPassword);

        await fetch('/profile/update-user', {
            method: 'POST',
            body: formData
        }).then(async response => {
            if (response.status === 200) {
                this.setState({ isSaved: true });
            } else if (response.status === 400) {
                const errors = await response.json();
                console.log(errors);
                //TODO implement logic
            }
        });
    }
    onNameChanged(newUserName) {
        if (newUserName === "" || newUserName === null) return { isSuccess: false, error: 'Вы не ввели имя' };
        if (newUserName.length < 4) return { isSuccess: false, error: 'Имя должно быть длиной не менее 3 символов' };
        if (newUserName.length > 20) return { isSuccess: false, error: 'Имя должно быть длиной не более 20 символов' };

        const user = this.state.user;
        user.username = newUserName;

        this.setState({ isSaved: false, user: user });
        return { isSuccess: true, error: null };
    }

    onEmailChanged(newEmail) {
        const emailRegexp = /^(?!\s)([0-9]|[a-zA-Z])+@.+\..+$/ig;
        if (!emailRegexp.test(newEmail)) return { isSuccess: false, error: 'Вы ввели некорректный адрес эл.почты' };

        const user = this.state.user;
        user.email = newEmail;

        this.setState({ isSaved: false, user: user });
        return { isSuccess: true, error: null};
    }

    onPasswordChanged(newPassword, field) {
        const user = this.state.user;

        const passRegexp = /(?!\s)(?=.*([а-яА-ЯёЁ]|[a-zA-Z]))(?=.*[0-9]).{6,30}/ig;
        if (field === "newpassword") {
            if (!passRegexp.test(newPassword))
                return {
                    isSuccess: false,
                    error: 'Пароль должен содержать от 6 до 30 символов, не иметь пробела, состоять из цифр и букв'
                };
            user.password = newPassword;
            this.setState({ isSaved: false, user: user });
            return { isSuccess: true, error: null };
        }
        else if (field === "oldpassword") {
            user.oldPassword = newPassword;
            this.setState({ isSaved: false, user: user });
            return { isSuccess: true, error: null };
        } else window.location.reload();
    }
}

class UserNameInput extends React.Component {
    constructor(props) {
        super(props);

        this.state = { isChanged: false, isSuccess: false, error: null };

        this.onBlur = this.onBlur.bind(this);
    }

    render() {
        return(
        <div className="form-group">
            {this.state.isChanged
                ? this.state.isSuccess
                    ? null
                    : <h4 className="text-danger">{this.state.error}</h4>
                : null
            }
            <label>Имя пользователя</label>
            <input type="text" className="form-control" name="username"
                defaultValue={this.props.defaultUsername} onBlur={e => this.onBlur(e)} />
        </div>)
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
                {this.state.isChanged
                    ? this.state.isSuccess
                        ? null
                        : <h4 className="text-danger">{this.state.error}</h4>
                    : null
                }
                <label>Эл. почта</label>
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
    constructor(props) {
        super(props);

        this.state = { isChanged: false, isSuccess: false, error: null };

        this.onBlur = this.onBlur.bind(this);
    }

    render() {
        return (
            <div>
                {this.state.isChanged
                    ? this.state.isSuccess
                        ? null
                        : <h3 className="text-danger">{this.state.error}</h3>
                    : null
                }
            <div className="form-group">
                    <label>Старый пароль</label>
                    {this.state.isChanged
                        ? this.state.isSuccess
                            ? <input type="password" className="form-control" name="oldpassword"
                                onBlur={e => this.onBlur(e)} />
                            : <input type="password" className="form-control" name="oldpassword" readOnly disabled/>
                        : <input type="password" className="form-control" name="oldpassword"
                            onBlur={e => this.onBlur(e)} />
                    }
            </div>
            <div className="form-group">
                <label>Новый пароль</label>
                    <input type="password" className="form-control" name="newpassword"
                        onBlur={e=>this.onBlur(e)} />
            </div>
            </div>);
    }

    onBlur(event) {
        const elem = event.target;
        const value = elem.value, fieldType = elem.name;
        const { isSuccess, error } = this.props.handleChange(value, fieldType);
        this.setState({ isChanged: true, isSuccess: isSuccess, error: error })
    }
}