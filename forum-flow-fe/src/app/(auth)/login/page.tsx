"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { loginSchema, LoginFormData } from "@/lib/validations/auth";
import { axiosClient } from "@/lib/axios";
import { useAuthStore } from "@/store/useAuthStore";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

export default function LoginPage() {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    setServerError(null);
    try {
      const response = await axiosClient.post("/auth/login", data);
      const resData = response.data;

      // Lưu vào Zustand Store (tự động lưu LocalStorage qua persist)
      setAuth(resData.accessToken, {
        userId: resData.userId,
        username: resData.username,
        displayName: resData.displayName,
        email: data.email,
        role: resData.role,
      });

      router.push("/");
    } catch (err: unknown) {
      const errorObj = err as {
        response?: { data?: { error?: { message?: string } | string; message?: string } };
      };
      const apiErr = errorObj.response?.data?.error;
      const msg =
        typeof apiErr === "object"
          ? apiErr?.message
          : apiErr || errorObj.response?.data?.message || "Đăng nhập thất bại. Vui lòng thử lại!";
      setServerError(msg || "Đăng nhập thất bại. Vui lòng thử lại!");
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 dark:bg-zinc-900 px-4">
      <div className="w-full max-w-md p-8 bg-white dark:bg-zinc-800 rounded-xl shadow-lg border border-gray-100 dark:border-zinc-700">
        <h1 className="text-2xl font-bold mb-2 text-center text-zinc-900 dark:text-zinc-50">
          Đăng nhập Forum-Flow
        </h1>
        <p className="text-gray-500 text-sm text-center mb-6">
          Nhập thông tin tài khoản của bạn để tiếp tục
        </p>

        {serverError && (
          <div className="mb-4 p-3 text-sm text-red-600 bg-red-50 dark:bg-red-950/50 dark:text-red-400 rounded-lg border border-red-200 dark:border-red-900">
            {serverError}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1 text-zinc-700 dark:text-zinc-300">
              Email
            </label>
            <Input
              type="email"
              placeholder="name@example.com"
              {...register("email")}
              disabled={isSubmitting}
            />
            {errors.email && (
              <p className="text-xs text-red-500 mt-1">{errors.email.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium mb-1 text-zinc-700 dark:text-zinc-300">
              Mật khẩu
            </label>
            <Input
              type="password"
              placeholder="••••••••"
              {...register("password")}
              disabled={isSubmitting}
            />
            {errors.password && (
              <p className="text-xs text-red-500 mt-1">{errors.password.message}</p>
            )}
          </div>

          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? "Đang xử lý..." : "Đăng nhập"}
          </Button>
        </form>

        <p className="text-center text-sm text-gray-500 mt-6">
          Chưa có tài khoản?{" "}
          <Link href="/register" className="text-indigo-600 dark:text-indigo-400 font-medium hover:underline">
            Đăng ký ngay
          </Link>
        </p>
      </div>
    </div>
  );
}
