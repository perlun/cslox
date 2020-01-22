#!/usr/bin/env ruby

INDENT = ' ' * 4
VISIBILITY = 'internal'

class Type
  attr_reader :class_name
  attr_reader :fields

  def initialize(class_name, *fields)
    @class_name = class_name
    @fields = fields.flatten
  end
end

class Field
  attr_reader :type
  attr_reader :name

  def initialize(type, name)
    @type = type
    @name = name
  end
end

def define_visitor(base_name, types)
  stream = StringIO.new

  stream.puts "#{INDENT * 2}#{VISIBILITY} interface Visitor<R>"
  stream.puts "#{INDENT * 2}{"

  types.each { |t|
    stream.puts "#{INDENT * 3}R Visit#{t.class_name}#{base_name}(#{t.class_name} #{base_name.downcase});"
  }

  stream.puts "#{INDENT * 2}}"

  stream.string
end

def define_type(base_name, class_name, fields)
  stream = StringIO.new

  stream.puts "#{INDENT * 2}#{VISIBILITY} class #{class_name} : #{base_name}"
  stream.puts "#{INDENT * 2}{"

  # Fields.
  fields.each { |f|
    stream.puts("#{INDENT * 3}#{VISIBILITY} readonly #{f.type} #{f.name};");
  }

  stream.puts

  # Constructor.
  constructor_fields = fields.map { |f|
    "#{f.type} #{f.name}"
  }.join(', ')
  stream.puts "#{INDENT * 3}#{VISIBILITY} #{class_name}(#{constructor_fields}) {"

  # Store parameters in fields.
  fields.each { |f|
    stream.puts "#{INDENT * 4}this.#{f.name} = #{f.name};"
  }

  stream.puts "#{INDENT * 3}}";

  stream.puts

  # Visitor pattern implementation
  stream.puts "#{INDENT * 3}#{VISIBILITY} override R Accept<R>(Visitor<R> visitor)"
  stream.puts "#{INDENT * 3}{"
  stream.puts "#{INDENT * 4}return visitor.Visit#{class_name}#{base_name}(this);"
  stream.puts "#{INDENT * 3}}"

  stream.puts "#{INDENT * 2}}"

  stream
end

def define_ast(output_dir, base_name, types)
  visitor_content = define_visitor(base_name, types)

  inner_classes = types.map { |type|
    define_type(base_name, type.class_name, type.fields)
  }

  inner_classes_content = inner_classes
    .map(&:string)
    .join("\n")
    .rstrip

  path = File.join(output_dir, base_name + ".cs");
  File.write(path, <<~EOF)
using System.Collections.Generic;

namespace CSLox
{
#{INDENT}#{VISIBILITY} abstract class #{base_name}
#{INDENT}{
#{visitor_content}
#{inner_classes_content}

#{INDENT * 2}#{VISIBILITY} abstract R Accept<R>(Visitor<R> visitor);
#{INDENT}}
}
  EOF
end

if ARGV.count != 1
  puts "Usage: generate_ast <output directory>"
  exit 1
end

output_dir = ARGV.pop

#
# Certain names like _params and _operator are prefixed, since they
# clash with C# reserved words.
#

define_ast(output_dir, "Expr", [
  Type.new('Assign', Field.new('Token', 'name'), Field.new('Expr', 'value')),
  Type.new('Binary', [
    Field.new('Expr', 'left'),
    Field.new('Token', '_operator'),
    Field.new('Expr', 'right')
  ]),
  Type.new('Call', [
    Field.new('Expr', 'callee'),
    Field.new('Token', 'paren'),
    Field.new('List<Expr>', 'arguments')
  ]),
  Type.new('Grouping', Field.new('Expr', 'expression')),
  Type.new('Literal', Field.new('object', 'value')),
  Type.new('Logical', [
    Field.new('Expr', 'left'),
    Field.new('Token', '_operator'),
    Field.new('Expr', 'right')
  ]),
  Type.new('Unary', [
    Field.new('Token', '_operator'),
    Field.new('Expr', 'right')
  ]),
  Type.new('Variable', Field.new('Token', 'name'))
])

define_ast(output_dir, "Stmt", [
  Type.new('Block', Field.new('List<Stmt>', 'statements')),
  Type.new('Expression', Field.new('Expr', 'expression')),
  Type.new('Function', [
    Field.new('Token', 'name'),
    Field.new('List<Token>', '_params'),
    Field.new('List<Stmt>', 'body')
  ]),
  Type.new('If', [
    Field.new('Expr', 'condition'),
    Field.new('Stmt', 'thenBranch'),
    Field.new('Stmt', 'elseBranch')
  ]),
  Type.new('Print', Field.new('Expr', 'expression')),
  Type.new('Return', Field.new('Token', 'keyword'), Field.new('Expr', 'value')),
  Type.new('Var', [
    Field.new('Token', 'name'),
    Field.new('Expr', 'initializer')
  ]),
  Type.new('While', Field.new('Expr', 'condition'), Field.new('Stmt', 'body'))
])
