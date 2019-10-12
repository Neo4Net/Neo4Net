using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Test
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using MultipleFailureException = org.junit.runners.model.MultipleFailureException;
	using Statement = org.junit.runners.model.Statement;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;

	public class TestData<T> : TestRule
	{
		 [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		 public class Title : System.Attribute
		 {
			 private readonly TestData<T> _outerInstance;

			 public Title;
			 {
			 }

			  internal string value;

			 public Title( public Title, String value )
			 {
				 this.Title = Title;
				 this.value = value;
			 }
		 }

		 public interface Producer<T>
		 {
			  T Create( GraphDefinition graph, string title, string documentation );

			  void Destroy( T product, bool successful );
		 }

		 public static TestData<T> ProducedThrough<T>( Producer<T> transformation )
		 {
			  Objects.requireNonNull( transformation );
			  return new TestData<T>( transformation );
		 }

		 public virtual T Get()
		 {
			  return Get( true );
		 }

		 private sealed class Lazy
		 {
			  internal volatile object ProductOrFactory;

			  internal Lazy( GraphDefinition graph, string title, string documentation )
			  {
					ProductOrFactory = new Factory( graph, title, documentation );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") <T> T get(Producer<T> producer, boolean create)
			  internal T Get<T>( Producer<T> producer, bool create )
			  {
					object result = ProductOrFactory;
					if ( result is Factory )
					{
						 lock ( this )
						 {
							  if ( ( result = ProductOrFactory ) is Factory )
							  {
									ProductOrFactory = result = ( ( Factory ) result ).Create( producer, create );
							  }
						 }
					}
					return ( T ) result;
			  }
		 }

		 private sealed class Factory
		 {
			  internal readonly GraphDefinition Graph;
			  internal readonly string Title;
			  internal readonly string Documentation;

			  internal Factory( GraphDefinition graph, string title, string documentation )
			  {
					this.Graph = graph;
					this.Title = title;
					this.Documentation = documentation;
			  }

			  internal object Create<T1>( Producer<T1> producer, bool create )
			  {
					return create ? producer.Create( Graph, Title, Documentation ) : null;
			  }
		 }

		 private readonly Producer<T> _producer;
		 private readonly ThreadLocal<Lazy> _product = new InheritableThreadLocal<Lazy>();

		 private TestData( Producer<T> producer )
		 {
			  this._producer = producer;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, final org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Title title = description.getAnnotation(Title.class);
			  Title title = description.getAnnotation( typeof( Title ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.annotations.Documented doc = description.getAnnotation(org.neo4j.kernel.impl.annotations.Documented.class);
			  Documented doc = description.getAnnotation( typeof( Documented ) );
			  GraphDescription.Graph g = description.getAnnotation( typeof( GraphDescription.Graph ) );
			  if ( g == null )
			  {
					g = description.TestClass.getAnnotation( typeof( GraphDescription.Graph ) );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final GraphDescription graph = GraphDescription.create(g);
			  GraphDescription graph = GraphDescription.Create( g );
			  return new StatementAnonymousInnerClass( this, @base, description, title, doc, graph );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly TestData<T> _outerInstance;

			 private Statement @base;
			 private Description _description;
			 private Title _title;
			 private Documented _doc;
			 private Neo4Net.Test.GraphDescription _graph;

			 public StatementAnonymousInnerClass( TestData<T> outerInstance, Statement @base, Description description, Title title, Documented doc, Neo4Net.Test.GraphDescription graph )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._description = description;
				 this._title = title;
				 this._doc = doc;
				 this._graph = graph;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  _outerInstance.product.set( Create( _graph, _title == null ? null : _title.value(), _doc == null ? null : _doc.value(), _description.MethodName ) );
				  try
				  {
						try
						{
							 @base.evaluate();
						}
						catch ( Exception err )
						{
							 try
							 {
								  outerInstance.destroy( outerInstance.get( false ), false );
							 }
							 catch ( Exception sub )
							 {
								  IList<Exception> failures = new List<Exception>();
								  if ( err is MultipleFailureException )
								  {
										( ( IList<Exception> )failures ).AddRange( ( ( MultipleFailureException ) err ).Failures );
								  }
								  else
								  {
										failures.Add( err );
								  }
								  failures.Add( sub );
								  throw new MultipleFailureException( failures );
							 }
							 throw err;
						}
						outerInstance.destroy( outerInstance.get( false ), false );
				  }
				  finally
				  {
						_outerInstance.product.set( null );
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private void destroy(@SuppressWarnings("hiding") T product, boolean successful)
		 private void Destroy( T product, bool successful )
		 {
			  if ( product != default( T ) )
			  {
					_producer.destroy( product, successful );
			  }
		 }

		 private T Get( bool create )
		 {
			  Lazy lazy = _product.get();
			  if ( lazy == null )
			  {
					if ( create )
					{
						 throw new System.InvalidOperationException( "Not in test case" );
					}
					return default( T );
			  }
			  return lazy.Get( _producer, create );
		 }

		 private const string EMPTY = "";

		 private static Lazy Create( GraphDescription graph, string title, string doc, string methodName )
		 {
			  if ( !string.ReferenceEquals( doc, null ) )
			  {
					if ( string.ReferenceEquals( title, null ) )
					{
						 // standard javadoc means of finding a title
						 int dot = doc.IndexOf( '.' );
						 if ( dot > 0 )
						 {
							  title = doc.Substring( 0, dot );
							  if ( title.Contains( "\n" ) )
							  {
									title = null;
							  }
							  else
							  {
									title = title.Trim();
									doc = doc.Substring( dot + 1 );
							  }
						 }
					}
					string[] lines = doc.Split( "\n", true );
					int indent = int.MaxValue;
					int start = 0;
					int end = 0;
					for ( int i = 0; i < lines.Length; i++ )
					{
						 if ( EMPTY.Equals( lines[i].Trim() ) )
						 {
							  lines[i] = EMPTY;
							  if ( start == i )
							  {
									end = ++start; // skip initial blank lines
							  }
						 }
						 else
						 {
							  for ( int j = 0; j < lines[i].Length; j++ )
							  {
									if ( !char.IsWhiteSpace( lines[i][j] ) )
									{
										 indent = Math.Min( indent, j );
										 break;
									}
							  }
							  end = i; // skip blank lines at the end
						 }
					}
					if ( end == lines.Length )
					{
						 end--; // all lines were empty
					}
					// If there still is no title, and the first line looks like a
					// title, take the first line as title
					if ( string.ReferenceEquals( title, null ) && start < end && EMPTY.Equals( lines[start + 1] ) )
					{
						 title = lines[start].Trim();
						 start += 2;
					}
					StringBuilder documentation = new StringBuilder();
					for ( int i = start; i <= end; i++ )
					{
						 documentation.Append( EMPTY.Equals( lines[i] ) ? EMPTY : lines[i].Substring( indent ) ).Append( "\n" );
					}
					doc = documentation.ToString();
			  }
			  else
			  {
					doc = EMPTY;
			  }
			  if ( string.ReferenceEquals( title, null ) )
			  {
					title = methodName.Replace( "_", " " );
			  }
			  return new Lazy( graph, title, doc );
		 }
	}

}