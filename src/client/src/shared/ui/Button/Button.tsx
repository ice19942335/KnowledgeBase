import type { ButtonHTMLAttributes } from "react";
import styles from "./Button.module.css";

type ButtonVariant = "primary" | "secondary" | "danger";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
}

const variantClassName: Record<ButtonVariant, string> = {
  primary: "",
  secondary: styles.secondary,
  danger: styles.danger,
};

export function Button({ variant = "primary", className, ...rest }: ButtonProps) {
  const classes = [styles.button, variantClassName[variant], className]
    .filter(Boolean)
    .join(" ");

  return <button className={classes} {...rest} />;
}
