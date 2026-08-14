import { lazy, Suspense } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import PageLoader from "./components/loader/PageLoader.jsx";
import "./components/loader/PageLoader.css";
import { ExamRecordsProvider } from "./context/ExamRecordsContext.jsx";

const Home = lazy(() => import("./pages/public/home/Home.jsx"));
const Login = lazy(() => import("./pages/public/auth/login/Login.jsx"));
const Register = lazy(() => import("./pages/public/auth/register/Register.jsx"));
const Contact = lazy(() => import("./pages/public/contact/Contact.jsx"));

const OnboardingStep1 = lazy(() => import("./pages/onboarding/step-one/OnboardingStep1.jsx"));
const OnboardingStep2 = lazy(() => import("./pages/onboarding/step-two/OnboardingStep2.jsx"));

const Dashboard = lazy(() => import("./pages/student/dashboard/Dashboard.jsx"));
const Subjects = lazy(() => import("./pages/student/subjects/Subjects.jsx"));
const Exams = lazy(() => import("./pages/student/exams/Exams.jsx"));
const ExamCreate = lazy(() => import("./pages/student/exam-create/ExamCreate.jsx"));
const ExamTake = lazy(() => import("./pages/student/exam-take/ExamTake.jsx"));
const StudyPlans = lazy(() => import("./pages/student/study-plans/StudyPlans.jsx"));
const Analytics = lazy(() => import("./pages/student/analytics/Analytics.jsx"));
const Achievements = lazy(() => import("./pages/student/achievements/Achievements.jsx"));
const Profile = lazy(() => import("./pages/student/profile/Profile.jsx"));

const MainDashboard = lazy(() => import("./pages/admin/dashboard/MainDashboard.jsx"));
const AdminSubjects = lazy(() => import("./pages/admin/subjects/AdminSubjects.jsx"));
const AdminLessons = lazy(() => import("./pages/admin/lessons/AdminLessons.jsx"));
const AdminExams = lazy(() => import("./pages/admin/exams/AdminExams.jsx"));
const AdminUsers = lazy(() => import("./pages/admin/users/AdminUsers.jsx"));
const AdminMessages = lazy(() => import("./pages/admin/messages/AdminMessages.jsx"));
const AdminAnalytics = lazy(() => import("./pages/admin/analytics/AdminAnalytics.jsx"));
const AdminAchievements = lazy(() => import("./pages/admin/achievements/AdminAchievements.jsx"));
const AdminProfile = lazy(() => import("./pages/admin/profile/AdminProfile.jsx"));
const ForgotPassword = lazy(() => import("./pages/ForgotPasswordPage.jsx"));
//OtpVerificationPage
const OtpVerificationPage = lazy(() => import("./pages/OtpVerificationPage.jsx"));
//ResetPasswordPage
const ResetPasswordPage = lazy(() => import("./pages/ResetPasswordPage.jsx"));


const NotFound = lazy(() => import("./pages/not-found/NotFound.jsx"));

import {
  GuestOnlyRoute,
  StudentRoute,
  OnboardingRoute,
  AdminRoute,
} from "./routes/ProtectedRoute.jsx";

const App = () => (
  <BrowserRouter>
    <ExamRecordsProvider>
      <Suspense fallback={<PageLoader />}>
        <Routes>
          <Route element={<GuestOnlyRoute />}>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/contact" element={<Contact />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/otp-verification" element={<OtpVerificationPage />} />
            <Route path="/reset-password" element={<ResetPasswordPage />} />
          </Route>

          <Route element={<OnboardingRoute />}>
            <Route path="/onboarding/1" element={<OnboardingStep1 />} />
            <Route path="/onboarding/2" element={<OnboardingStep2 />} />
          </Route>

          <Route element={<StudentRoute />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/subjects" element={<Subjects />} />
            <Route path="/exams" element={<Exams />} />
            <Route path="/exams/new" element={<ExamCreate />} />
            <Route path="/exams/take" element={<ExamTake />} />
            <Route path="/plans" element={<StudyPlans />} />
            <Route path="/analytics" element={<Analytics />} />
            <Route path="/achievements" element={<Achievements />} />
            <Route path="/profile" element={<Profile />} />
          </Route>

          <Route element={<AdminRoute />}>
            <Route path="/admin-dashboard" element={<MainDashboard />} />
            <Route path="/admin-subjects" element={<AdminSubjects />} />
            <Route path="/admin-lessons" element={<AdminLessons />} />
            <Route path="/admin-exams" element={<AdminExams />} />
            <Route path="/admin-users" element={<AdminUsers />} />
            <Route path="/admin-messages" element={<AdminMessages />} />
            <Route path="/admin-analytics" element={<AdminAnalytics />} />
            <Route path="/admin-achievements" element={<AdminAchievements />} />
            <Route path="/admin-profile" element={<AdminProfile />} />
          </Route>

          <Route path="*" element={<NotFound />} />
        </Routes>
      </Suspense>
    </ExamRecordsProvider>  </BrowserRouter>
);

export default App;
