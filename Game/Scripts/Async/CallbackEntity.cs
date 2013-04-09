using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryEngine;

namespace CryGameCode.Async
{
	using CallbackTask = Task<object>;

	/// <summary>
	/// Allows for passing the results of asynchronous tasks back to the main game thread.
	/// </summary>
	[Entity(Flags = EntityClassFlags.Invisible)]
	public class CallbackEntity : Entity
	{
		/// <summary>
		/// Defines the result of a task as scheduled back to the main thread.
		/// </summary>
		/// <typeparam name="T">The type of the result.</typeparam>
		public class Callback<T>
		{
			private T m_result;
			private AggregateException m_exceptions;

			/// <summary>
			/// Indicates whether the task completed successfully.
			/// </summary>
			public bool Successful { get; private set; }

			/// <summary>
			/// Retrieves the result of the task.
			/// </summary>
			/// <exception cref="GameCallbackException">
			/// Thrown if the task was unsuccessful.
			/// </exception>
			public T Result
			{
				get
				{
					if (Successful)
						return m_result;
					else
						throw new GameCallbackException("Exception(s) thrown in scheduled task, callback result is not valid.",
							m_exceptions);
				}
			}

			/// <summary>
			/// Tries to retrieve the result of the task.
			/// </summary>
			/// <param name="result"></param>
			/// <returns></returns>
			public bool TryGetResult(out T result)
			{
				result = Successful ? m_result : default(T);
				return Successful;
			}

			public Callback(CallbackTask task)
			{
				Successful = task.Status == TaskStatus.RanToCompletion;

				if (Successful)
					m_result = (T)task.Result;
				else
				{
					var exceptions = task.Exception;
					m_exceptions = exceptions.InnerException as AggregateException;

					if (m_exceptions == null)
						throw new GameCallbackException("Internal error occurred", exceptions);
				}
			}
		}

		protected override bool OnRemove()
		{
			if (m_current.Count > 0)
			{
				Debug.LogWarning("Can't remove task scheduler with running tasks");
				return false;
			}

			return true;
		}

		public override void OnUpdate()
		{
			m_done.Clear();

			foreach (var state in m_current)
				TryExecute(state);

			foreach (var task in m_done)
				m_current.Remove(task);

			if (m_current.Count == 0)
				ReceiveUpdates = false;
		}

		/// <summary>
		/// Register a new asynchronous task to have its result delivered to the main thread.
		/// </summary>
		/// <typeparam name="T">The type of the result.</typeparam>
		/// <param name="task">The asynchronous Task to execute in parallel to the game thread.</param>
		/// <param name="action">The callback to be executed on the game thread once the Task is complete.</param>
		public void Register<T>(Task<T> task, Action<Callback<T>> action)
		{
			var process = task.ContinueWith(t => (object)t.Result);
			var state = new CallbackState(process, (t) => action(new Callback<T>(t)));

			m_current.Add(state);
			ReceiveUpdates = true;
		}

		// Polls a state, executes and handles exceptions if necessary
		private void TryExecute(CallbackState state)
		{
			var task = state.Task;

			if (task.IsCompleted)
			{
				m_done.Add(state);

				try
				{
					state.Action(task);
				}
				catch
				{
					foreach (var doneState in m_done)
						m_current.Remove(doneState);

					m_done.Clear();
					throw;
				}
			}
		}

		// Internal representation of a scheduled task
		private class CallbackState
		{
			public CallbackTask Task { get; private set; }
			public Action<CallbackTask> Action { get; private set; }

			public CallbackState(CallbackTask task, Action<CallbackTask> action)
			{
				Task = task;
				Action = action;
			}
		}

		private List<CallbackState> m_current = new List<CallbackState>();
		private List<CallbackState> m_done = new List<CallbackState>();
	}

	/// <summary>
	/// The exception thrown when errors occur in the CallbackEntity.
	/// </summary>
	public class GameCallbackException : Exception
	{
		public GameCallbackException(string message, Exception exception)
			: base(message, exception)
		{
		}
	}
}
