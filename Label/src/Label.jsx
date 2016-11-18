import React, {Component, PropTypes } from "react";
import Tooltip from "dnn-tooltip";
import "./style.less";

const tooltipStyle = {
    float: "left",
    position: "static"
};

class Label extends Component {
    constructor() {
        super();
    }

    render() {
        const {props} = this;
        const tooltipMessages = props.tooltipMessage instanceof Array ? props.tooltipMessage : [props.tooltipMessage];
        return (
            <div className={"dnn-label" + (props.className ? (" " + props.className) : "") + (" " + props.labelType) } style={props.style}>
                <label htmlFor={props.labelFor}>{props.label}</label>
                <Tooltip
                    messages={tooltipMessages}
                    type="info"
                    tooltipPlace={props.tooltipPlace}
                    rendered={tooltipMessages.length > 0}
                    style={Object.assign(tooltipStyle, props.tooltipStyle)}/>
                    {props.extra}
            </div>
        );
    }
}

Label.propTypes = {
    label: PropTypes.string,
    className: PropTypes.string,
    labelFor: PropTypes.string,
    tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    tooltipPlace: PropTypes.string,
    tooltipStyle: PropTypes.object,
    labelType: PropTypes.oneOf(["inline", "block"]),
    style: PropTypes.object,
    extra: PropTypes.node
};
Label.defaultProps = {
    labelType: "block",
    className: ""
};
export default Label;