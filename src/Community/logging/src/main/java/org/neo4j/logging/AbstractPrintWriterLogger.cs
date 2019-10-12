using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Logging
{

	/// <summary>
	/// An abstract <seealso cref="Logger"/> implementation, which takes care of locking and flushing.
	/// </summary>
	public abstract class AbstractPrintWriterLogger : Logger
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void bulk(@Nonnull Consumer<Logger> consumer);
		public abstract void Bulk( Consumer<Logger> consumer );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void log(@Nonnull String format, @Nullable Object... arguments);
		public abstract void Log( string format, params object[] arguments );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void log(@Nonnull String message, @Nonnull Throwable throwable);
		public abstract void Log( string message, Exception throwable );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void log(@Nonnull String message);
		public abstract void Log( string message );
		 private readonly System.Func<PrintWriter> _writerSupplier;
		 private readonly object @lock;
		 private readonly bool _autoFlush;

		 /// <param name="writerSupplier"> A <seealso cref="Supplier"/> for the <seealso cref="PrintWriter"/> that logs should be written to </param>
		 /// <param name="lock">           An object that will be used to synchronize all writes on </param>
		 /// <param name="autoFlush">      Whether to flush the writer after each log message is written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: protected AbstractPrintWriterLogger(@Nonnull Supplier<java.io.PrintWriter> writerSupplier, @Nonnull Object lock, boolean autoFlush)
		 protected internal AbstractPrintWriterLogger( Supplier<PrintWriter> writerSupplier, object @lock, bool autoFlush )
		 {
			  this._writerSupplier = writerSupplier;
			  this.@lock = @lock;
			  this._autoFlush = autoFlush;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message)
		 public override void Log( string message )
		 {
			  requireNonNull( message, "message must not be null" );
			  PrintWriter writer;
			  lock ( @lock )
			  {
					writer = _writerSupplier.get();
					WriteLog( writer, message );
			  }
			  MaybeFlush( writer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Log( string message, Exception throwable )
		 {
			  requireNonNull( message, "message must not be null" );
			  if ( throwable == null )
			  {
					Log( message );
					return;
			  }
			  PrintWriter writer;
			  lock ( @lock )
			  {
					writer = _writerSupplier.get();
					WriteLog( writer, message, throwable );
			  }
			  MaybeFlush( writer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String format, @Nullable Object... arguments)
		 public override void Log( string format, params object[] arguments )
		 {
			  requireNonNull( format, "format must not be null" );
			  if ( arguments == null || arguments.Length == 0 )
			  {
					Log( format );
					return;
			  }
			  string message = string.format( format, arguments );
			  PrintWriter writer;
			  lock ( @lock )
			  {
					writer = _writerSupplier.get();
					WriteLog( writer, message );
			  }
			  MaybeFlush( writer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Logger> consumer)
		 public override void Bulk( Consumer<Logger> consumer )
		 {
			  requireNonNull( consumer, "consumer must not be null" );
			  PrintWriter writer;
			  lock ( @lock )
			  {
					writer = _writerSupplier.get();
					consumer.accept( GetBulkLogger( writer, @lock ) );
			  }
			  MaybeFlush( writer );
		 }

		 /// <summary>
		 /// Invoked when a log line should be written. This method will only be called synchronously (whilst a lock is held
		 /// on the lock object provided during construction).
		 /// </summary>
		 /// <param name="writer"> the writer to write to </param>
		 /// <param name="message"> the message to write </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: protected abstract void writeLog(@Nonnull PrintWriter writer, @Nonnull String message);
		 protected internal abstract void WriteLog( PrintWriter writer, string message );

		 /// <summary>
		 /// Invoked when a log line should be written. This method will only be called synchronously (whilst a lock is held
		 /// on the lock object provided during construction).
		 /// </summary>
		 /// <param name="writer"> the writer to write to </param>
		 /// <param name="message"> the message to write </param>
		 /// <param name="throwable"> the exception to append to the log message </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: protected abstract void writeLog(@Nonnull PrintWriter writer, @Nonnull String message, @Nonnull Throwable throwable);
		 protected internal abstract void WriteLog( PrintWriter writer, string message, Exception throwable );

		 /// <summary>
		 /// Return a variant of the logger which will output to the specified writer (whilst holding a lock on the specified
		 /// object) in a bulk manner (no flushing, etc).
		 /// </summary>
		 /// <param name="writer"> the writer to write to </param>
		 /// <param name="lock"> the object on which to lock </param>
		 /// <returns> a new logger for bulk writes </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: protected abstract Logger getBulkLogger(@Nonnull PrintWriter writer, @Nonnull Object lock);
		 protected internal abstract Logger GetBulkLogger( PrintWriter writer, object @lock );

		 private void MaybeFlush( PrintWriter writer )
		 {
			  if ( _autoFlush )
			  {
					writer.flush();
			  }
		 }
	}

}