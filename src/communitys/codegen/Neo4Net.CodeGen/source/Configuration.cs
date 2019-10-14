using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.CodeGen.Source
{

	using Neo4Net.CodeGen;

	internal class Configuration
	{
		 private IList<Processor> _processors = new List<Processor>();
		 private ISet<SourceCode> _flags = EnumSet.noneOf( typeof( SourceCode ) );
		 private IList<string> _options = new List<string>();
		 private IList<SourceVisitor> _sourceVisitors = new List<SourceVisitor>();
		 private IList<WarningsHandler> _warningsHandlers = new List<WarningsHandler>();
		 internal SourceCompiler_Factory Compiler = JdkCompiler.FACTORY;

		 public virtual Configuration WithAnnotationProcessor( Processor processor )
		 {
			  _processors.Add( processor );
			  return this;
		 }

		 public virtual Configuration WithFlag( SourceCode flag )
		 {
			  _flags.Add( flag );
			  return this;
		 }

		 public virtual Configuration WithOptions( params string[] opts )
		 {
			  if ( opts != null )
			  {
					Collections.addAll( _options, opts );
			  }
			  return this;
		 }

		 public virtual Configuration WithSourceVisitor( SourceVisitor visitor )
		 {
			  _sourceVisitors.Add( visitor );
			  return this;
		 }

		 public virtual Configuration WithWarningsHandler( WarningsHandler handler )
		 {
			  _warningsHandlers.Add( handler );
			  return this;
		 }

		 public virtual IEnumerable<string> Options()
		 {
			  return _options;
		 }

		 public virtual void Processors( JavaCompiler.CompilationTask task )
		 {
			  task.Processors = _processors;
		 }

		 public virtual Locale Locale()
		 {
			  return null;
		 }

		 public virtual Charset Charset()
		 {
			  return null;
		 }

		 public virtual Writer ErrorWriter()
		 {
			  return null;
		 }

		 public virtual BaseUri SourceBase()
		 {
			  return BaseUri.DefaultSourceBase;
		 }

		 public virtual bool IsSet( SourceCode flag )
		 {
			  return _flags != null && _flags.Contains( flag );
		 }

		 public virtual void Visit( TypeReference reference, StringBuilder source )
		 {
			  foreach ( SourceVisitor visitor in _sourceVisitors )
			  {
					visitor.VisitSource( reference, source );
			  }
		 }

		 public virtual WarningsHandler WarningsHandler()
		 {
			  if ( _warningsHandlers.Count == 0 )
			  {
					return WarningsHandler_Fields.NoWarningsHandler;
			  }
			  if ( _warningsHandlers.Count == 1 )
			  {
					return _warningsHandlers[0];
			  }
			  return new WarningsHandler_Multiplex( _warningsHandlers.ToArray() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SourceCompiler sourceCompilerFor(org.neo4j.codegen.CodeGenerationStrategy<?> strategy) throws org.neo4j.codegen.CodeGenerationStrategyNotSupportedException
		 public virtual SourceCompiler SourceCompilerFor<T1>( CodeGenerationStrategy<T1> strategy )
		 {
			  return Compiler.sourceCompilerFor( this, strategy );
		 }

		 public virtual void UseJdkJavaCompiler()
		 {
			  Compiler = null;
		 }
	}

}