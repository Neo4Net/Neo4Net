using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Test.subprocess
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.proc.ProcessUtil.getClassPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.proc.ProcessUtil.getClassPathList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.proc.ProcessUtil.getJavaExecutable;

	[Serializable]
	public abstract class SubProcess<T, P>
	{
		 private const long SERIAL_VERSION_UID = -6084373832996850958L;

		 private interface NoInterface
		 {
			  // Used when no interface is declared
		 }

		 // by default will inherit output destinations for subprocess from current process
		 private const bool INHERIT_OUTPUT_DEFAULT_VALUE = true;

		 private Type<T> _t;
		 [NonSerialized]
		 private bool _inheritOutput = INHERIT_OUTPUT_DEFAULT_VALUE;
		 [NonSerialized]
		 private readonly System.Predicate<string> _classPathFilter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "unchecked", "rawtypes" }) public SubProcess(System.Predicate<String> classPathFilter, boolean inheritOutput)
		 public SubProcess( System.Predicate<string> classPathFilter, bool inheritOutput )
		 {
			  this._inheritOutput = inheritOutput;
			  if ( this.GetType().BaseType != typeof(SubProcess) )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new System.InvalidCastException( typeof( SubProcess ).FullName + " may only be extended one level " );
			  }
			  Type me = this.GetType();
			  while ( me.BaseType != typeof( SubProcess ) )
			  {
					me = me.BaseType;
			  }
			  Type type = ( ( ParameterizedType ) me.GenericSuperclass ).ActualTypeArguments[0];
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "hiding" }) Class<T> t;
			  Type t = typeof( T );
			  if ( type is Type )
			  {
					t = ( Type<T> ) type;
			  }
			  else if ( type is ParameterizedType )
			  {
					t = ( Type<T> )( ( ParameterizedType ) type ).RawType;
			  }
			  else
			  {
					throw new System.InvalidCastException( "Illegal type parameter " + type );
			  }
			  if ( t == typeof( object ) )
			  {
					t = ( Type ) typeof( NoInterface );
			  }
			  if ( !t.IsInterface )
			  {
					throw new System.InvalidCastException( t + " is not an interface" );
			  }
			  if ( t.IsAssignableFrom( this.GetType() ) || t == typeof(NoInterface) )
			  {
					this._t = t;
			  }
			  else
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new System.InvalidCastException( this.GetType().FullName + " must implement declared interface " + t );
			  }
			  this._classPathFilter = classPathFilter;
		 }

		 public SubProcess() : this(null, INHERIT_OUTPUT_DEFAULT_VALUE)
		 {
		 }

		 public virtual T Start( P parameter )
		 {
			  DispatcherTrapImpl callback;
			  try
			  {
					callback = new DispatcherTrapImpl( this, parameter );
			  }
			  catch ( RemoteException e )
			  {
					throw new Exception( "Failed to create local RMI endpoint.", e );
			  }
			  Process process;
			  string pid;
			  Dispatcher dispatcher;
			  try
			  {
					string java = JavaExecutable.ToString();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					process = Start( _inheritOutput, java, "-ea", "-Xmx1G", "-Djava.awt.headless=true", "-cp", ClassPath(), typeof(SubProcess).FullName, Serialize(callback) );
					pid = GetPid( process );
					// if IO was not inherited by current process we need to pipe error and input stream to corresponding
					// target streams
					if ( !_inheritOutput )
					{
						 Pipe( "[" + ToString() + ":" + pid + "] ", process.ErrorStream, ErrorStreamTarget() );
						 Pipe( "[" + ToString() + ":" + pid + "] ", process.InputStream, InputStreamTarget() );
					}
					dispatcher = callback.Get( process );
			  }
			  catch ( Exception t )
			  {
					throw new Exception( "Failed to start sub process", t );
			  }
			  finally
			  {
					try
					{
						 UnicastRemoteObject.unexportObject( callback, true );
					}
					catch ( RemoteException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
			  }
			  requireNonNull( dispatcher );
			  Handler handler = new Handler( t, dispatcher, process, "<" + ToString() + ":" + pid + ">" );
			  return t.cast( Proxy.newProxyInstance( t.ClassLoader, new Type[]{ t }, Live( handler ) ) );
		 }

		 protected internal virtual PrintStream ErrorStreamTarget()
		 {
			  return System.err;
		 }

		 protected internal virtual PrintStream InputStreamTarget()
		 {
			  return System.out;
		 }

		 private string ClassPath()
		 {
			  if ( _classPathFilter == null )
			  {
					return ClassPath;
			  }
			  Stream<string> stream = ClassPathList.stream();
			  return stream.filter( _classPathFilter ).collect( Collectors.joining( File.pathSeparator ) );
		 }

		 private static Process Start( bool inheritOutput, params string[] args )
		 {
			  ProcessBuilder builder = new ProcessBuilder( args );
			  if ( inheritOutput )
			  {
					// We can not simply use builder.inheritIO here because
					// that will also inherit input which will be closed in case of background execution of main process.
					// Closed input stream will cause immediate exit from a subprocess liveloop.
					// And we use background execution in scripts and on CI server.
					builder.redirectError( ProcessBuilder.Redirect.INHERIT ).redirectOutput( ProcessBuilder.Redirect.INHERIT );
			  }
			  try
			  {
					return builder.start();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Failed to start sub process", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void startup(P parameter) throws Throwable;
		 protected internal abstract void Startup( P parameter );

		 public void Shutdown()
		 {
			  Shutdown( true );
		 }

		 protected internal virtual void Shutdown( bool normal )
		 {
			  Environment.Exit( 0 );
		 }

		 public static void Stop( object subprocess )
		 {
			  ( ( Handler ) Proxy.getInvocationHandler( subprocess ) ).Stop( null, 0 );
		 }

		 public static void Stop( object subprocess, long timeout, TimeUnit unit )
		 {
			  ( ( Handler ) Proxy.getInvocationHandler( subprocess ) ).Stop( unit, timeout );
		 }

		 public static void Kill( object subprocess )
		 {
			  ( ( Handler ) Proxy.getInvocationHandler( subprocess ) ).Kill( true );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Throwable
		 public static void Main( string[] args )
		 {
			  if ( args.Length != 1 )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new System.ArgumentException( "Needs to be started from " + typeof( SubProcess ).FullName );
			  }
			  DispatcherTrap trap = Deserialize( args[0] );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: SubProcess<?, Object> subProcess = trap.getSubProcess();
			  SubProcess<object, object> subProcess = trap.SubProcess;
			  subProcess.DoStart( trap.Trap( new DispatcherImpl( subProcess ) ) );
		 }

		 [NonSerialized]
		 private volatile bool _alive;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doStart(P parameter) throws Throwable
		 private void DoStart( P parameter )
		 {
			  _alive = true;
			  Startup( parameter );
			  LiveLoop();
		 }

		 private void DoStop( bool normal )
		 {
			  _alive = false;
			  Shutdown( normal );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void liveLoop() throws Exception
		 private void LiveLoop()
		 {
			  while ( _alive )
			  {
					for ( int i = System.in.available(); i >= 0; i-- )
					{
						 if ( Console.Read() == -1 )
						 {
							  // Parent process exited, die with it
							  DoStop( false );
						 }
						 Thread.Sleep( 1 );
					}
			  }
		 }

		 private static readonly System.Reflection.FieldInfo _pid;
		 static SubProcess()
		 {
			  System.Reflection.FieldInfo pid;
			  try
			  {
					pid = Type.GetType( "java.lang.UNIXProcess" ).getDeclaredField( "pid" );
					pid.Accessible = true;
			  }
			  catch ( Exception )
			  {
					pid = null;
			  }
			  _pid = pid;
		 }

		 private int _lastPid;

		 private string GetPid( Process process )
		 {
			  if ( _pid != null )
			  {
					try
					{
						 return _pid.get( process ).ToString();
					}
					catch ( Exception )
					{
						 // handled by lastPid++
					}
			  }
			  return Convert.ToString( _lastPid++ );
		 }

		 private class PipeTask
		 {
			  internal readonly string Prefix;
			  internal readonly Stream Source;
			  internal readonly PrintStream Target;
			  internal StringBuilder Line;

			  internal PipeTask( string prefix, Stream source, PrintStream target )
			  {
					this.Prefix = prefix;
					this.Source = source;
					this.Target = target;
					Line = new StringBuilder();
			  }

			  internal virtual bool Pipe()
			  {
					try
					{
						 sbyte[] data = new sbyte[Math.Max( 1, Source.available() )];
						 int bytesRead = Source.Read( data, 0, data.Length );
						 if ( bytesRead == -1 )
						 {
							  PrintLastLine();
							  return false;
						 }
						 if ( bytesRead < data.Length )
						 {
							  data = Arrays.copyOf( data, bytesRead );
						 }
						 ByteBuffer chars = ByteBuffer.wrap( data );
						 while ( chars.hasRemaining() )
						 {
							  char c = ( char ) chars.get();
							  Line.Append( c );
							  if ( c == '\n' )
							  {
									Print();
							  }
						 }
					}
					catch ( IOException )
					{
						 PrintLastLine();
						 return false;
					}
					return true;
			  }

			  internal virtual void PrintLastLine()
			  {
					if ( Line.Length > 0 )
					{
						 Line.Append( '\n' );
						 Print();
					}
			  }

			  internal virtual void Print()
			  {
					Target.print( Prefix + Line.ToString() );
					Line = new StringBuilder();
			  }
		 }

		 private class PipeThread : Thread
		 {
			 internal bool InstanceFieldsInitialized = false;

			 public PipeThread()
			 {
				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			 internal virtual void InitializeInstanceFields()
			 {
				 Name = this.GetType().Name;
			 }

			  internal readonly CopyOnWriteArrayList<PipeTask> Tasks = new CopyOnWriteArrayList<PipeTask>();

			  public override void Run()
			  {
					while ( true )
					{
						 IList<PipeTask> done = new List<PipeTask>();
						 foreach ( PipeTask task in Tasks )
						 {
							  if ( !task.Pipe() )
							  {
									done.Add( task );
							  }
						 }
						 if ( done.Count > 0 )
						 {
							  Tasks.removeAll( done );
						 }
						 if ( Tasks.Empty )
						 {
							  lock ( typeof( PipeThread ) )
							  {
									if ( Tasks.Empty )
									{
										 _piper = null;
										 return;
									}
							  }
						 }
						 try
						 {
							  Thread.Sleep( 10 );
						 }
						 catch ( InterruptedException )
						 {
							  Thread.interrupted();
						 }
					}
			  }
		 }

		 private static PipeThread _piper;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static void pipe(final String prefix, final java.io.InputStream source, final java.io.PrintStream target)
		 private static void Pipe( string prefix, Stream source, PrintStream target )
		 {
			  lock ( typeof( PipeThread ) )
			  {
					if ( _piper == null )
					{
						 _piper = new PipeThread();
						 _piper.Start();
					}
					_piper.tasks.add( new PipeTask( prefix, source, target ) );
			  }
		 }

		 private interface DispatcherTrap : Remote
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object trap(Dispatcher dispatcher) throws java.rmi.RemoteException;
			  object Trap( Dispatcher dispatcher );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: SubProcess<?, Object> getSubProcess() throws java.rmi.RemoteException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  SubProcess<object, object> SubProcess { get; }
		 }

		 private class DispatcherTrapImpl : UnicastRemoteObject, DispatcherTrap
		 {
			  internal object Parameter;
			  internal volatile Dispatcher Dispatcher;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private SubProcess<?, ?> process;
			  internal SubProcess<object, ?> Process;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: DispatcherTrapImpl(SubProcess<?, ?> process, Object parameter) throws java.rmi.RemoteException
			  internal DispatcherTrapImpl<T1>( SubProcess<T1> process, object parameter ) : base()
			  {
					this.Process = process;
					this.Parameter = parameter;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: Dispatcher get(@SuppressWarnings("hiding") Process process)
			  internal virtual Dispatcher Get( Process process )
			  {
					while ( Dispatcher == null && process.Alive )
					{
						 try
						 {
							  Thread.Sleep( 10 );
						 }
						 catch ( InterruptedException )
						 {
							  Thread.CurrentThread.Interrupt();
						 }
					}
					return Dispatcher;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public synchronized Object trap(@SuppressWarnings("hiding") Dispatcher dispatcher)
			  public override object Trap( Dispatcher dispatcher )
			  {
				  lock ( this )
				  {
						if ( this.Dispatcher != null )
						{
							 throw new System.InvalidOperationException( "Dispatcher already trapped!" );
						}
						this.Dispatcher = dispatcher;
						return Parameter;
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public SubProcess<?, Object> getSubProcess()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  public virtual SubProcess<object, object> SubProcess
			  {
				  get
				  {
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: return (SubProcess<?, Object>) process;
						return ( SubProcess<object, object> ) Process;
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("restriction") private static String serialize(DispatcherTrapImpl obj)
		 private static string Serialize( DispatcherTrapImpl obj )
		 {
			  MemoryStream os = new MemoryStream();
			  try
			  {
					ObjectOutputStream oos = new ObjectOutputStream( os );
					oos.writeObject( RemoteObject.toStub( obj ) );
					oos.close();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Broken implementation!", e );
			  }
			  return Base64.Encoder.encodeToString( os.toByteArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("restriction") private static DispatcherTrap deserialize(String data) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static DispatcherTrap Deserialize( string data )
		 {
			  return ( DispatcherTrap ) ( new ObjectInputStream( new MemoryStream( Base64.Decoder.decode( data ) ) ) ).readObject();
		 }

		 private interface Dispatcher : Remote
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stop() throws java.rmi.RemoteException;
			  void Stop();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object dispatch(String name, String[] types, Object[] args) throws Throwable;
			  object Dispatch( string name, string[] types, object[] args );
		 }

		 private static InvocationHandler Live( Handler handler )
		 {
			  try
			  {
					lock ( typeof( Handler ) )
					{
						 if ( _live == null )
						 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<Handler> handlers = live = new java.util.HashSet<>();
							  ISet<Handler> handlers = _live = new HashSet<Handler>();
							  Runtime.Runtime.addShutdownHook( new Thread( () => killAll(handlers) ) );
						 }
						 _live.Add( handler );
					}
			  }
			  catch ( System.NotSupportedException )
			  {
					handler.Kill( false );
					throw new System.InvalidOperationException( "JVM is shutting down!" );
			  }
			  return handler;
		 }

		 private static void Dead( Handler handler )
		 {
			  lock ( typeof( Handler ) )
			  {
					try
					{
						 if ( _live != null )
						 {
							  _live.remove( handler );
						 }
					}
					catch ( System.NotSupportedException )
					{
						 // ok, already dead
					}
			  }
		 }

		 private static void KillAll( ISet<Handler> handlers )
		 {
			  lock ( typeof( Handler ) )
			  {
					if ( handlers.Count > 0 )
					{
						 foreach ( Handler handler in handlers )
						 {
							  try
							  {
									handler.Process.exitValue();
							  }
							  catch ( IllegalThreadStateException )
							  {
									handler.Kill( false );
							  }
						 }
					}
					_live = Collections.emptySet();
			  }
		 }

		 private static ISet<Handler> _live;

		 private class Handler : InvocationHandler
		 {
			  internal readonly Dispatcher Dispatcher;
			  internal readonly Process Process;
			  internal readonly Type Type;
			  internal readonly string Repr;

			  internal Handler( Type type, Dispatcher dispatcher, Process process, string repr )
			  {
					this.Type = type;
					this.Dispatcher = dispatcher;
					this.Process = process;
					this.Repr = repr;
			  }

			  public override string ToString()
			  {
					return Repr;
			  }

			  internal virtual void Kill( bool wait )
			  {
					Process.destroy();
					if ( wait )
					{
						 Dead( this );
						 Await( Process );
					}
			  }

			  internal virtual int Stop( TimeUnit unit, long timeout )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(unit == null ? 0 : 1);
					System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( unit == null ? 0 : 1 );
					Thread stopper = new Thread(() =>
					{
					latch.Signal();
					try
					{
						Dispatcher.stop();
					}
					catch ( RemoteException )
					{
						Process.destroy();
					}
					});
					stopper.Start();
					try
					{
						 latch.await();
						 timeout = DateTimeHelper.CurrentUnixTimeMillis() + (unit == null ? 0 : unit.toMillis(timeout));
						 while ( stopper.IsAlive && DateTimeHelper.CurrentUnixTimeMillis() < timeout )
						 {
							  Thread.Sleep( 1 );
						 }
					}
					catch ( InterruptedException )
					{
						 // handled by exit
					}
					if ( stopper.IsAlive )
					{
						 stopper.Interrupt();
					}
					Dead( this );
					return Await( Process );
			  }

			  internal static int Await( Process process )
			  {
					return ( new ProcessStreamHandler( process, true ) ).waitForResult();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object invoke(Object proxy, Method method, Object[] args) throws Throwable
			  public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					try
					{
						 if ( method.DeclaringClass == Type )
						 {
							  return Dispatch( method, args );
						 }
						 else if ( method.DeclaringClass == typeof( object ) )
						 {
							  return method.invoke( this, args );
						 }
						 else
						 {
							  throw new System.NotSupportedException( method.ToString() );
						 }
					}
					catch ( ServerError ex )
					{
						 throw ex.detail;
					}
					catch ( RemoteException ex )
					{
						 throw new ConnectionDisruptedException( ex );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Object dispatch(Method method, Object[] args) throws Throwable
			  internal virtual object Dispatch( System.Reflection.MethodInfo method, object[] args )
			  {
					Type[] @params = method.ParameterTypes;
					string[] types = new string[@params.Length];
					for ( int i = 0; i < types.Length; i++ )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 types[i] = @params[i].FullName;
					}
					return Dispatcher.dispatch( method.Name, types, args );
			  }
		 }

		 private class DispatcherImpl : UnicastRemoteObject, Dispatcher
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final transient SubProcess<?, ?> subprocess;
			  [NonSerialized]
			  internal readonly SubProcess<object, ?> Subprocess;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected DispatcherImpl(SubProcess<?, ?> subprocess) throws java.rmi.RemoteException
			  protected internal DispatcherImpl<T1>( SubProcess<T1> subprocess ) : base()
			  {
					this.Subprocess = subprocess;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object dispatch(String name, String[] types, Object[] args) throws Throwable
			  public override object Dispatch( string name, string[] types, object[] args )
			  {
					Type[] @params = new Type[types.Length];
					for ( int i = 0; i < @params.Length; i++ )
					{
						 @params[i] = Type.GetType( types[i] );
					}
					try
					{
						 return Subprocess.t.GetMethod( name, @params ).invoke( Subprocess, args );
					}
					catch ( IllegalAccessException e )
					{
						 throw new System.InvalidOperationException( e );
					}
					catch ( InvocationTargetException e )
					{
						 throw e.TargetException;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws java.rmi.RemoteException
			  public override void Stop()
			  {
					Subprocess.doStop( true );
			  }
		 }
	}

}