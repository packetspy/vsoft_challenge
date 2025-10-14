<template>
  <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-400 py-8 flex items-center justify-center px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div class="bg-white shadow-lg rounded-xl p-8">
        <div>
          <h2 class="mt-4 p-2 text-center text-3xl font-extrabold text-transparent bg-clip-text bg-gradient-to-r from-violet-400 to-rose-600">Task Management</h2>
          <p class="mt-2 text-center text-sm text-gray-600">Please enter your credentials below.</p>
        </div>
        <form class="mt-8 space-y-6" @submit.prevent="handleLogin">
          <div>
            <label for="username" class="text-sm text-gray-700">Username:</label>
            <input
              id="username"
              v-model="newUser.username"
              name="username"
              type="text"
              required
              class="relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-emerald-500 focus:border-emerald-500 focus:z-10 sm:text-sm transition duration-200"
              placeholder="Username"
            />
          </div>
          <div>
            <label for="email" class="text-sm text-gray-700">Email:</label>
            <input
              id="email"
              v-model="newUser.email"
              name="email"
              type="text"
              required
              class="relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-emerald-500 focus:border-emerald-500 focus:z-10 sm:text-sm transition duration-200"
              placeholder="email"
            />
          </div>
          <div>
            <label for="password" class="text-sm text-gray-700">Password:</label>
            <input
              id="password"
              v-model="newUser.password"
              name="password"
              type="password"
              required
              class="relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-emerald-500 focus:border-emerald-500 focus:z-10 sm:text-sm transition duration-200"
              placeholder="Password"
            />
          </div>
          <div>
            <label for="confirmPassword" class="text-sm text-gray-700">Confirmation Password:</label>
            <input
              id="confirmPassword"
              v-model="newUser.confirmPassword"
              name="confirmPassword"
              type="password"
              required
              class="relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-emerald-500 focus:border-emerald-500 focus:z-10 sm:text-sm transition duration-200"
              placeholder="Confirm Password"
            />
          </div>

          <div v-if="error" class="text-red-600 text-sm text-center">
            {{ error }}
          </div>

          <div v-if="success" class="text-green-600 text-sm text-center success-message">
            {{ success }}
          </div>

          <div>
            <button
              type="submit"
              :disabled="authStore.isLoading"
              class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-emerald-600 hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500 disabled:opacity-50"
            >
              <span class="absolute left-0 inset-y-0 flex items-center pl-3">
                <svg class="h-5 w-5 text-emerald-200 group-hover:text-emerald-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fill-rule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clip-rule="evenodd" />
                </svg>
              </span>
              Register
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import { useUserStore } from '@/stores/userStore';
import type { CreateRandomUsersRequest, NewUser } from '@/types';

const router = useRouter();
const authStore = useAuthStore();
const userStore = useUserStore();

const newUser = ref<NewUser>({
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
});

const userModel = ref<CreateRandomUsersRequest>({
  amount: 10,
  userNameMask: 'user_{{random}}',
});

const error = ref<string>('');
const success = ref<string>('');

const handleLogin = async (): Promise<void> => {
  try {
    error.value = '';
    await userStore.registerNewUser(newUser.value);
    success.value = 'User registered successfully';
    newUser.value = { username: '', email: '', password: '', confirmPassword: '' };
  } catch (err) {
    error.value = 'Failed to register new user';
  }
};
</script>
