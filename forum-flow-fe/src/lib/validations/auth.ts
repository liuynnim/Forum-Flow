import { z } from "zod";

export const loginSchema = z.object({
  email: z
    .string()
    .min(1, "Email bắt buộc nhập!")
    .email("Email không đúng định dạng!"),
  password: z
    .string()
    .min(1, "Mật khẩu bắt buộc nhập!")
    .min(6, "Mật khẩu tối thiểu 6 ký tự!"),
});

export type LoginFormData = z.infer<typeof loginSchema>;

export const registerSchema = z
  .object({
    username: z
      .string()
      .min(1, "Tên tài khoản (username) bắt buộc nhập!")
      .min(3, "Username tối thiểu 3 ký tự!")
      .max(50, "Username tối đa 50 ký tự!")
      .regex(/^[a-zA-Z0-9_-]+$/, "Username chỉ chứa chữ cái, số, dấu gạch dưới hoặc gạch nối!"),
    displayName: z
      .string()
      .min(1, "Tên hiển thị bắt buộc nhập!")
      .min(2, "Tên hiển thị tối thiểu 2 ký tự!")
      .max(100, "Tên hiển thị tối đa 100 ký tự!"),
    email: z
      .string()
      .min(1, "Email bắt buộc nhập!")
      .email("Email không đúng định dạng!"),
    password: z
      .string()
      .min(1, "Mật khẩu bắt buộc nhập!")
      .min(6, "Mật khẩu tối thiểu 6 ký tự!"),
    confirmPassword: z
      .string()
      .min(1, "Xác nhận mật khẩu bắt buộc nhập!"),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Mật khẩu xác nhận không khớp!",
    path: ["confirmPassword"],
  });

export type RegisterFormData = z.infer<typeof registerSchema>;

