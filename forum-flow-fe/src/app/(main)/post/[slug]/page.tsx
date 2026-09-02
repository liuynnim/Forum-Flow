type Props = {
  params: Promise<{ slug: string }>;
};

export default async function PostDetailPage({ params }: Props) {
  const { slug } = await params;
  return (
    <div className="container mx-auto p-6">
      <h1 className="text-3xl font-bold">Bài viết: {slug}</h1>
      <p className="mt-4 text-gray-600">
        Nội dung chi tiết bài viết sẽ hiển thị tại đây (SSR)...
      </p>
    </div>
  );
}
