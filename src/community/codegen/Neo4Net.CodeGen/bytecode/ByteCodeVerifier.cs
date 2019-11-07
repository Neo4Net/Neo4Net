using System;
using System.Collections.Generic;

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
namespace Neo4Net.CodeGen.ByteCode
{
	using ClassReader = org.objectweb.asm.ClassReader;
	using Opcodes = org.objectweb.asm.Opcodes;
	using Type = org.objectweb.asm.Type;
	using AbstractInsnNode = org.objectweb.asm.tree.AbstractInsnNode;
	using ClassNode = org.objectweb.asm.tree.ClassNode;
	using MethodNode = org.objectweb.asm.tree.MethodNode;
	using Analyzer = org.objectweb.asm.tree.analysis.Analyzer;
	using AnalyzerException = org.objectweb.asm.tree.analysis.AnalyzerException;
	using BasicValue = org.objectweb.asm.tree.analysis.BasicValue;
	using Frame = org.objectweb.asm.tree.analysis.Frame;
	using SimpleVerifier = org.objectweb.asm.tree.analysis.SimpleVerifier;
	using Value = org.objectweb.asm.tree.analysis.Value;
	using CheckClassAdapter = org.objectweb.asm.util.CheckClassAdapter;
	using Textifier = org.objectweb.asm.util.Textifier;
	using TraceMethodVisitor = org.objectweb.asm.util.TraceMethodVisitor;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.ClassReader.SKIP_DEBUG;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") class ByteCodeVerifier implements ByteCodeChecker, Neo4Net.codegen.CodeGeneratorOption
	internal class ByteCodeVerifier : ByteCodeChecker, CodeGeneratorOption
	{
		 /// <summary>
		 /// Invoked by <seealso cref="ByteCode.load(string)"/> to load this class.
		 /// </summary>
		 /// <returns> an instance of this class, if all dependencies are available. </returns>
		 internal static CodeGeneratorOption LoadVerifier()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  if ( typeof( Analyzer ).FullName.Length == 0 || typeof( CheckClassAdapter ).FullName.Length == 0 )
			  {
					throw new AssertionError( "This code is here to ensure the optional ASM classes are on the classpath" );
			  }
			  return new ByteCodeVerifier();
		 }

		 /// <summary>
		 /// Add this verification step to the configuration, if applicable.
		 /// </summary>
		 /// <param name="target">
		 ///         the configuration to add this verification step to. </param>
		 public override void ApplyTo( object target )
		 {
			  if ( target is Configuration )
			  {
					( ( Configuration ) target ).WithBytecodeChecker( this );
			  }
		 }

		 /// <summary>
		 /// Check the bytecode from one round of bytecode generation.
		 /// </summary>
		 /// <param name="classpathLoader">
		 ///         the ClassLoader to use for loading classes from the classpath. </param>
		 /// <param name="byteCodes">
		 ///         the bytecodes generated in this round. </param>
		 /// <exception cref="CompilationFailureException">
		 ///         if any issue is discovered in the verification. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void check(ClassLoader classpathLoader, java.util.Collection<Neo4Net.codegen.ByteCodes> byteCodes) throws Neo4Net.codegen.CompilationFailureException
		 public override void Check( ClassLoader classpathLoader, ICollection<ByteCodes> byteCodes )
		 {
			  IList<ClassNode> classes = new List<ClassNode>( byteCodes.Count );
			  IList<Failure> failures = new List<Failure>();
			  // load (and verify) the structure of the generated classes
			  foreach ( ByteCodes byteCode in byteCodes )
			  {
					try
					{
						 classes.Add( ClassNode( byteCode.Bytes() ) );
					}
					catch ( Exception e )
					{
						 failures.Add( new Failure( e, e.ToString() ) );
					}
			  }
			  // if there are problems with the structure of the generated classes,
			  // we are not going to be able to verify their methods
			  if ( failures.Count > 0 )
			  {
					throw CompilationFailure( failures );
			  }
			  // continue with verifying the methods of the classes
			  AssignmentChecker check = new AssignmentChecker( classpathLoader, classes );
			  foreach ( ClassNode clazz in classes )
			  {
					Verify( check, clazz, failures );
			  }
			  if ( failures.Count > 0 )
			  {
					throw CompilationFailure( failures );
			  }
		 }

		 /// <summary>
		 /// Verify the methods of a single class.
		 /// </summary>
		 /// <param name="check">
		 ///         a helper for verifying assignments. </param>
		 /// <param name="clazz">
		 ///         the class to check the methods of. </param>
		 /// <param name="failures">
		 ///         where any detected errors are added. </param>
		 private static void Verify( AssignmentChecker check, ClassNode clazz, IList<Failure> failures )
		 {
			  Verifier verifier = new Verifier( clazz, check );
			  foreach ( MethodNode method in clazz.methods )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.objectweb.asm.tree.analysis.Analyzer<?> analyzer = new org.objectweb.asm.tree.analysis.Analyzer<>(verifier);
					Analyzer<object> analyzer = new Analyzer<object>( verifier );
					try
					{
						 analyzer.analyze( clazz.name, method );
					}
					catch ( Exception cause )
					{
						 failures.Add( new Failure( cause, DetailedMessage( cause.Message, method, analyzer.Frames, cause is AnalyzerException ? ( ( AnalyzerException ) cause ).node : null ) ) );
					}
			  }
		 }

		 private static ClassNode ClassNode( ByteBuffer bytecode )
		 {
			  sbyte[] bytes;
			  if ( bytecode.hasArray() )
			  {
					bytes = bytecode.array();
			  }
			  else
			  {
					bytes = new sbyte[bytecode.limit()];
					bytecode.get( bytes );
			  }
			  ClassNode classNode = new ClassNode();
			  ( new ClassReader( bytes ) ).accept( new CheckClassAdapter( classNode, false ), SKIP_DEBUG );
			  return classNode;
		 }

		 private static CompilationFailureException CompilationFailure( IList<Failure> failures )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<javax.tools.Diagnostic<?>> diagnostics = new java.util.ArrayList<>(failures.size());
			  IList<Diagnostic<object>> diagnostics = new List<Diagnostic<object>>( failures.Count );
			  foreach ( Failure failure in failures )
			  {
					diagnostics.Add( new BytecodeDiagnostic( failure.Message ) );
			  }
			  CompilationFailureException exception = new CompilationFailureException( diagnostics );
			  foreach ( Failure failure in failures )
			  {
					exception.addSuppressed( failure.Cause );
			  }
			  return exception;
		 }

		 private class Failure
		 {
			  internal readonly Exception Cause;
			  internal readonly string Message;

			  internal Failure( Exception cause, string message )
			  {
					this.Cause = cause;
					this.Message = message;
			  }
		 }

		 private static string DetailedMessage( string errorMessage, MethodNode method, Frame[] frames, AbstractInsnNode errorLocation )
		 {
			  StringWriter message = new StringWriter();
			  using ( PrintWriter @out = new PrintWriter( message ) )
			  {
					IList<int> localLengths = new List<int>();
					IList<int> stackLengths = new List<int>();
					foreach ( Frame frame in frames )
					{
						 if ( frame != null )
						 {
							  for ( int i = 0; i < frame.Locals; i++ )
							  {
									Insert( i, frame.getLocal( i ), localLengths );
							  }
							  for ( int i = 0; i < frame.StackSize; i++ )
							  {
									Insert( i, frame.getStack( i ), stackLengths );
							  }
						 }
					}
					Textifier formatted = new Textifier();
					TraceMethodVisitor mv = new TraceMethodVisitor( formatted );

					@out.println( errorMessage );
					@out.append( "\t\tin " ).append( method.name ).append( method.desc ).println();
					for ( int i = 0; i < method.instructions.size(); i++ )
					{
						 AbstractInsnNode insn = method.instructions.get( i );
						 insn.accept( mv );
						 Frame frame = frames[i];
						 @out.append( "\t\t" );
						 @out.append( insn == errorLocation ? ">>> " : "    " );
						 @out.format( "%05d [", i );
						 if ( frame == null )
						 {
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
							  Padding( @out, localLengths.GetEnumerator(), '?' );
							  @out.append( " : " );
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
							  Padding( @out, stackLengths.GetEnumerator(), '?' );
						 }
						 else
						 {
							  Emit( @out, localLengths, frame.getLocal, frame.Locals );
							  Padding( @out, localLengths.listIterator( frame.Locals ), '-' );
							  @out.append( " : " );
							  Emit( @out, stackLengths, frame.getStack, frame.StackSize );
							  Padding( @out, stackLengths.listIterator( frame.StackSize ), ' ' );
						 }
						 @out.print( "] : " );
						 @out.print( formatted.text.get( formatted.text.size() - 1 ) );
					}
					for ( int j = 0; j < method.tryCatchBlocks.size(); j++ )
					{
						 method.tryCatchBlocks.get( j ).accept( mv );
						 @out.print( " " + formatted.text.get( formatted.text.size() - 1 ) );
					}
			  }
			  return message.ToString();
		 }

		 private static void Emit( PrintWriter @out, IList<int> lengths, System.Func<int, Value> valueLookup, int values )
		 {
			  for ( int i = 0; i < values; i++ )
			  {
					if ( i > 0 )
					{
						 @out.append( ' ' );
					}
					string name = ShortName( valueLookup( i ).ToString() );
					for ( int pad = lengths[i] - name.Length; pad-- > 0; )
					{
						 @out.append( ' ' );
					}
					@out.append( name );
			  }
		 }

		 private static void Padding( PrintWriter @out, IEnumerator<int> lengths, char var )
		 {
			  while ( lengths.MoveNext() )
			  {
					if ( lengths.nextIndex() > 0 )
					{
						 @out.append( ' ' );
					}
					for ( int length = lengths.Current; length-- > 1; )
					{
						 @out.append( ' ' );
					}
					@out.append( var );
			  }
		 }

		 private static void Insert( int i, Value value, IList<int> values )
		 {
			  int length = ShortName( value.ToString() ).Length;
			  while ( i >= values.Count )
			  {
					values.Add( 1 );
			  }
			  if ( length > values[i] )
			  {
					values[i] = length;
			  }
		 }

		 private static string ShortName( string name )
		 {
			  int start = name.LastIndexOf( '/' );
			  int end = name.Length;
			  if ( name[end - 1] == ';' )
			  {
					end--;
			  }
			  return start == -1 ? name : name.Substring( start + 1, end - ( start + 1 ) );
		 }

		 // This class might look failed in the IDE, but javac will accept it
		 // The reason is because the base class has been re-written to work on old JVMs - generics have been dropped.
		 private class Verifier : SimpleVerifier
		 {
			  internal readonly AssignmentChecker Check;

			  internal Verifier( ClassNode clazz, AssignmentChecker check ) : base( ASM6, Type.getObjectType( clazz.name ), SuperClass( clazz ), Interfaces( clazz ), IsInterfaceNode( clazz ) )
			  {
					this.Check = check;
			  }

			  protected internal override bool IsAssignableFrom( Type target, Type value )
			  {
					return target, value.IsAssignableFrom( Check );
			  }

			  protected internal override bool IsSubTypeOf( BasicValue value, BasicValue expected )
			  {
					return base.IsSubTypeOf( value, expected ) || Check.invocableInterface( expected.Type, value.Type );
			  }

			  internal static Type SuperClass( ClassNode clazz )
			  {
					return clazz.superName == null ? null : Type.getObjectType( clazz.superName );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static java.util.List<org.objectweb.asm.Type> interfaces(org.objectweb.asm.tree.ClassNode clazz)
			  internal static IList<Type> Interfaces( ClassNode clazz )
			  {
					IList<Type> interfaces = new List<Type>( clazz.interfaces.size() );
					foreach ( string iFace in clazz.interfaces )
					{
						 interfaces.Add( Type.getObjectType( iFace ) );
					}
					return interfaces;
			  }
		 }

		 private class AssignmentChecker
		 {
			  internal readonly ClassLoader ClasspathLoader;
			  internal readonly IDictionary<Type, ClassNode> Classes = new Dictionary<Type, ClassNode>();

			  internal AssignmentChecker( ClassLoader classpathLoader, IList<ClassNode> classes )
			  {
					this.ClasspathLoader = classpathLoader;
					foreach ( ClassNode node in classes )
					{
						 this.Classes[Type.getObjectType( node.name )] = node;
					}
			  }

			  internal virtual bool InvocableInterface( Type target, Type value )
			  {
					// this method allows a bit too much through,
					// it really ought to only be used for the target type of INVOKEINTERFACE,
					// since any object type is allowed as target for INVOKEINTERFACE,
					// this allows fewer CHECKCAST instructions when using generics.
					ClassNode targetNode = Classes[target];
					if ( targetNode != null )
					{
						 if ( IsInterfaceNode( targetNode ) )
						 {
							  return value.Sort == Type.OBJECT || value.Sort == Type.ARRAY;
						 }
						 return false;
					}
					Type targetClass = GetClass( target );
					if ( targetClass.IsInterface )
					{
						 return value.Sort == Type.OBJECT || value.Sort == Type.ARRAY;
					}
					return false;
			  }

			  internal virtual bool IsAssignableFrom( Type target, Type value )
			  {
					if ( target.Equals( value ) )
					{
						 return true;
					}
					ClassNode targetNode = Classes[target];
					ClassNode valueNode = Classes[value];
					if ( targetNode != null && valueNode == null )
					{
						 // if the target is among the types we have generated and the value isn't, then
						 // the value type either doesn't exist, or it is defined in the class loader, and thus cannot
						 // be a subtype of the target type
						 return false;
					}
					else if ( valueNode != null )
					{
						 return IsAssignableFrom( target, valueNode );
					}
					else
					{
						 return GetClass( target ).IsAssignableFrom( GetClass( value ) );
					}
			  }

			  internal virtual bool IsAssignableFrom( Type target, ClassNode value )
			  {
					if ( value.superName != null && IsAssignableFrom( target, Type.getObjectType( value.superName ) ) )
					{
						 return true;
					}
					foreach ( string iFace in value.interfaces )
					{
						 if ( IsAssignableFrom( target, Type.getObjectType( iFace ) ) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  internal virtual Type GetClass( Type type )
			  {
					try
					{
						 if ( type.Sort == Type.ARRAY )
						 {
							  return Type.GetType( type.Descriptor.replace( '/', '.' ), false, ClasspathLoader );
						 }
						 return Type.GetType( type.ClassName, false, ClasspathLoader );
					}
					catch ( ClassNotFoundException e )
					{
						 throw new Exception( e.ToString() );
					}
			  }
		 }

		 private static bool IsInterfaceNode( ClassNode clazz )
		 {
			  return ( clazz.access & Opcodes.ACC_INTERFACE ) != 0;
		 }
	}


}